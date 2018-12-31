using Nexplorer.NexusClient.Core;

namespace Nexplorer.NexusClient.Request
{
    public class BlockRequest : BaseRequest { public BlockRequest(string blockHash) : base(0, "getblock", blockHash) { } }

    public class BlockCountRequest : BaseRequest { public BlockCountRequest() : base(0, "getblockcount") { } }

    public class BlockHashRequest : BaseRequest { public BlockHashRequest(int height) : base(0, "getblockhash", height) { } }

    public class TxRequest : BaseRequest { public TxRequest(string txHash) : base(0, "getglobaltransaction", txHash) { } }

    public class DifficultyRequest : BaseRequest { public DifficultyRequest() : base(0, "getdifficulty") { } }

    public class InfoRequest : BaseRequest { public InfoRequest() : base(0, "getinfo") { } }

    public class MemPoolRequest : BaseRequest { public MemPoolRequest() : base(0, "getrawmempool") { } }

    public class MiningInfoRequest : BaseRequest { public MiningInfoRequest() : base(0, "getmininginfo") { } }

    public class PeerInfoRequest : BaseRequest { public PeerInfoRequest() : base(0, "getpeerinfo") { } }

    public class SupplyRatesRequest : BaseRequest { public SupplyRatesRequest() : base(0, "getsupplyrates") { } }

    public class TrustKeysRequest : BaseRequest { public TrustKeysRequest() : base(0, "getnetworktrustkeys") { } }

    public class IsOrphanRequest : BaseRequest { public IsOrphanRequest(string blockHash) : base(0, "isorphan", blockHash) { } }
}