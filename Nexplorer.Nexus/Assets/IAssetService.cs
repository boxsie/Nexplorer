using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexplorer.Nexus.Assets.Models;

namespace Nexplorer.Nexus.Assets
{
    public interface IAssetService
    {
        Task<NexusResponse<AssetInfo>> GetAssetAsync(Asset asset, CancellationToken token = default);
        Task<NexusResponse<IEnumerable<AssetInfo>>> GetAssetHistoryAsync(Asset asset, CancellationToken token = default);
    }
}