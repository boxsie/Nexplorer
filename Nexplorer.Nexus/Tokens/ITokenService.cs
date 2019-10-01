using System.Threading;
using System.Threading.Tasks;
using Nexplorer.Nexus.Tokens.Models;

namespace Nexplorer.Nexus.Tokens
{
    public interface ITokenService
    {
        Task<TokenInfo> GetTokenInfo(Token token, CancellationToken cToken = default);
    }
}