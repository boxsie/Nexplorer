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
        private readonly INexusConnection _nxs;
        private readonly ILogger<LedgerService> _logger;

        public LedgerService(INexusConnection nxs, ILogger<LedgerService> logger)
        {
            _nxs = nxs;
            _logger = logger;
        }

        public async Task<NexusResponse<int>> GetHeightAsync(CancellationToken token = default)
        {
            var msg = await GetMiningInfoAsync(token);

            if (msg.HasError)
                _logger.LogError("Get blockchain height failed");
            
            return new NexusResponse<int>(msg.Result.Blocks, msg);
        }

        public async Task<NexusResponse<string>> GetBlockHashAsync(int height, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (height <= 0)
                _logger.LogError("Height must be greater than 0");

            var request = new NexusRequest(new Dictionary<string, string> {{"height", height.ToString()}});

            var msg = await _nxs.GetAsync<Block>("ledger/blockhash", request, token);

            if (msg.HasError || string.IsNullOrWhiteSpace(msg.Result.Hash))
                _logger.LogError($"Get block hash {height} failed");

            return new NexusResponse<string>(msg.Result.Hash, msg);
        }

        public async Task<NexusResponse<Block>> GetBlockAsync(string hash, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Hash must have a value");

            var msg = await GetBlockAsync((object) hash, txVerbosity, token);

            if (msg.HasError || string.IsNullOrWhiteSpace(msg.Result.Hash))
                _logger.LogError($"Get block {hash} failed");

            return msg;
        }

        public async Task<NexusResponse<Block>> GetBlockAsync(int height, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (height <= 0)
                _logger.LogError("Height must be greater than 0");

            var msg = await GetBlockAsync((object) height, txVerbosity, token);

            if (msg.HasError || string.IsNullOrWhiteSpace(msg.Result.Hash))
                _logger.LogError($"Get block {height} failed");

            return msg;
        }

        public async Task<NexusResponse<IEnumerable<Block>>> GetBlocksAsync(string hash, int count, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(hash))
                _logger.LogError("Hash must have a value");

            var msg = await GetBlocks(hash, count, txVerbosity, token);

            if (msg.HasError)
                _logger.LogError($"Get {count} blocks from {hash} failed");

            return msg;
        }

        public async Task<NexusResponse<IEnumerable<Block>>> GetBlocksAsync(int height, int count, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (height <= 0)
                _logger.LogError("Height must be greater than 0");

            var msg = await GetBlocks(height, count, txVerbosity, token);

            if (msg.HasError)
                _logger.LogError($"Get {count} blocks from {height} failed");

            return msg;
        }

        public async Task<NexusResponse<Transaction>> GetTransactionAsync(string hash, TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(hash))
                _logger.LogError("Hash must have a value");

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"hash", hash},
                {"verbose", ((int) txVerbosity).ToString()}
            });

            var msg = await _nxs.GetAsync<Transaction>("ledger/get/transaction", request, token);

            if (msg.HasError || string.IsNullOrWhiteSpace(msg.Result.Hash))
                _logger.LogError($"Get tx {hash} failed");

            return msg;
        }

        public async Task<NexusResponse<MiningInfo>> GetMiningInfoAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var info = await _nxs.GetAsync<MiningInfo>("ledger/get/mininginfo", null, token);

            if (info.HasError)
                _logger.LogError("Get mining info failed");

            return info;
        }

        private async Task<NexusResponse<Block>> GetBlockAsync(object retVal, TxVerbosity txVerbosity, CancellationToken token)
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

        private async Task<NexusResponse<IEnumerable<Block>>> GetBlocks(object retVal, int count, TxVerbosity txVerbosity, CancellationToken token)
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