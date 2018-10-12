using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class TxRequest : BaseRequest
    {
        public string TxHash { get; }

        public TxRequest(string txHash) : base(0, "getglobaltransaction", txHash)
        {
            TxHash = txHash;
        }
    }
}
