using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexplorer.Nexus.Enums;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Nexus.Ledger
{
    public interface ILedgerService
    {
        Task<NexusResponse<int>> GetHeightAsync(CancellationToken token = default);

        Task<NexusResponse<string>> GetBlockHashAsync(int height, CancellationToken token = default);

        Task<NexusResponse<Block>> GetBlockAsync(string hash, TxVerbosity txVerbosity = TxVerbosity.PubKeySign,
            CancellationToken token = default);

        Task<NexusResponse<Block>> GetBlockAsync(int height, TxVerbosity txVerbosity = TxVerbosity.PubKeySign,
            CancellationToken token = default);

        Task<NexusResponse<IEnumerable<Block>>> GetBlocksAsync(string hash, int count,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default);

        Task<NexusResponse<IEnumerable<Block>>> GetBlocksAsync(int height, int count,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default);

        Task<NexusResponse<Transaction>> GetTransactionAsync(string hash,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign,
            CancellationToken token = default);

        Task<NexusResponse<MiningInfo>> GetMiningInfoAsync(CancellationToken token = default);
    }
}