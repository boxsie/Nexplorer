using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Extensions;
using Nexplorer.Nexus.Tokens.Models;

namespace Nexplorer.Nexus.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly INexusConnection _nxs;
        private readonly ILogger<TokenService> _logger;

        public TokenService(INexusConnection nxs, ILogger<TokenService> logger)
        {
            _nxs = nxs;
            _logger = logger;
        }

        public async Task<NexusResponse<TokenInfo>> GetTokenInfo(Token token, CancellationToken cToken = default)
        {
            cToken.ThrowIfCancellationRequested();

            token.Validate();

            var (key, val) = token.GetKeyVal();

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"identifier", token.Identifier},
                {key, val},
                {"type", token.Type}
            });

            var msg = await _nxs.PostAsync<TokenInfo>("tokens/get", request, cToken);

            if (msg.HasError)
                _logger.LogError($"{token.Name} get failed");

            return msg;
        }
    }
}