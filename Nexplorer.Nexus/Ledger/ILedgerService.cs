using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexplorer.Nexus.Enums;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Nexus.Ledger
{
    public interface ILedgerService
    {
        Task<int?> GetHeightAsync(CancellationToken token = default);

        Task<string> GetBlockHashAsync(int height, CancellationToken token = default);

        Task<Block> GetBlockAsync(string hash, TxVerbosity txVerbosity = TxVerbosity.PubKeySign,
            CancellationToken token = default);

        Task<Block> GetBlockAsync(int height, TxVerbosity txVerbosity = TxVerbosity.PubKeySign,
            CancellationToken token = default);

        Task<IEnumerable<Block>> GetBlocksAsync(string hash, int count,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default);

        Task<IEnumerable<Block>> GetBlocksAsync(int height, int count,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default);

        Task<Transaction> GetTransactionAsync(string hash, TxVerbosity txVerbosity = TxVerbosity.PubKeySign,
            CancellationToken token = default);

        Task<MiningInfo> GetMiningInfoAsync(CancellationToken token = default);
    }
}