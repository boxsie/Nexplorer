using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Enums;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Nexus.Ledger
{
    public class LedgerService : ILedgerService
    {
        private const int GetBlocksDefaultCount = 10;
        private readonly INexusConnection _nxs;
        private readonly ILogger<LedgerService> _logger;

        public LedgerService(INexusConnection nxs, ILogger<LedgerService> logger)
        {
            _nxs = nxs;
            _logger = logger;
        }

        public async Task<int?> GetHeightAsync(CancellationToken token = default)
        {
            var info = await GetMiningInfoAsync(token);

            if (info == null)
                _logger.LogError("Get blockchain height failed");

            return info?.Blocks;
        }

        public async Task<string> GetBlockHashAsync(int height, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (height <= 0)
                _logger.LogError("Height must be greater than 0");

            var request = new NexusRequest(new Dictionary<string, string> {{"height", height.ToString()}});

            var block = await _nxs.GetAsync<Block>("ledger/blockhash", request, token);

            if (string.IsNullOrWhiteSpace(block?.Hash))
                _logger.LogError($"Get block hash {height} failed");

            return block?.Hash;
        }

        public async Task<Block> GetBlockAsync(string hash, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Hash must have a value");

            var block = await GetBlockAsync((object) hash, txVerbosity, token);

            if (string.IsNullOrWhiteSpace(block?.Hash))
                _logger.LogError($"Get block {hash} failed");

            return block;
        }

        public async Task<Block> GetBlockAsync(int height, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (height <= 0)
                _logger.LogError("Height must be greater than 0");

            var block = await GetBlockAsync((object) height, txVerbosity, token);

            if (string.IsNullOrWhiteSpace(block?.Hash))
                _logger.LogError($"Get block {height} failed");

            return block;
        }

        public async Task<IEnumerable<Block>> GetBlocksAsync(string hash, int count = GetBlocksDefaultCount,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(hash))
                _logger.LogError("Hash must have a value");

            var blocks = await GetBlocks(hash, count, txVerbosity, token);

            if (blocks == null)
                _logger.LogError($"Get {count} blocks from {hash} failed");

            return blocks;
        }

        public async Task<IEnumerable<Block>> GetBlocksAsync(int height, int count = GetBlocksDefaultCount, 
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (height <= 0)
                _logger.LogError("Height must be greater than 0");

            var blocks = await GetBlocks(height, count, txVerbosity, token);

            if (blocks == null)
                _logger.LogError($"Get {count} blocks from {height} failed");

            return blocks;
        }

        public async Task<Transaction> GetTransactionAsync(string hash, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(hash))
                _logger.LogError("Hash must have a value");

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"hash", hash},
                {"verbose", ((int) txVerbosity).ToString()}
            });

            var tx = await _nxs.GetAsync<Transaction>("ledger/get/transaction", request, token);

            if (string.IsNullOrWhiteSpace(tx?.Hash))
                _logger.LogError($"Get tx {hash} failed");

            return tx;
        }

        public async Task<MiningInfo> GetMiningInfoAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var info = await _nxs.GetAsync<MiningInfo>("ledger/get/mininginfo", null, token);

            if (info == null)
                _logger.LogError("Get mining info failed");

            return info;
        }

        private async Task<Block> GetBlockAsync(object retVal, TxVerbosity txVerbosity, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var useHash = retVal is string;

            var key = useHash ? "hash" : "height";
            var val = retVal.ToString();

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {key, val},
                {"verbose", ((int) txVerbosity).ToString()}
            });

            return await _nxs.GetAsync<Block>("ledger/get/block", request, token);
        }

        private async Task<IEnumerable<Block>> GetBlocks(object retVal, int count, TxVerbosity txVerbosity, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var useHash = retVal is string;

            var key = useHash ? "hash" : "height";
            var val = retVal.ToString();

            if (count > 1000)
            {
                count = 1000;
                _logger.LogWarning("Maximum block count is 1000");
            }

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {key, val},
                {"verbose", ((int) txVerbosity).ToString()},
                {"limit", count.ToString()}
            });

            return await _nxs.GetAsync<IEnumerable<Block>>("ledger/list/blocks", request, token);
        }
    }
}