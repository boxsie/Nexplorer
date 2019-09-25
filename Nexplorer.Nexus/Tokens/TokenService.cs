using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Accounts.Models;
using Nexplorer.Nexus.Extensions;
using Nexplorer.Nexus.Nexus;
using Nexplorer.Nexus.Tokens.Models;

namespace Nexplorer.Nexus.Tokens
{
    public class TokenService : NexusService
    {
        public TokenService(NexusNode node, ILogger<NexusService> log) : base(node, log) { }

        public async Task<T> CreateTokenAsync<T>(T token, NexusUser user, CancellationToken cToken = default) where T : Token
        {
            cToken.ThrowIfCancellationRequested();

            user.Validate();
            token.Validate();

            var param = new Dictionary<string, string>
            {
                {"pin", user.Pin.ToString()},
                {"session", user.GenesisId.Session},
                {"identifier", token.Identifier},
                {"name", token.Name},
                {"type", token.Type}
            };

            if (token is TokenRegister register)
                param.Add("supply", register.Supply.ToString(CultureInfo.InvariantCulture));

            var response = await PostAsync<NexusCreationResponse>("tokens/create", new NexusRequest(param), cToken);

            if (string.IsNullOrWhiteSpace(response?.Address))
                throw new InvalidOperationException($"{token.Name} creation failed");

            token.Address = response.Address;
            token.Tx = response.TxId;

            return token;
        }

        public async Task<TokenInfo> GetTokenInfo(Token token, CancellationToken cToken = default)
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

            var tokenInfo = await PostAsync<TokenInfo>("tokens/get", request, cToken);

            if (tokenInfo == null)
                throw new InvalidOperationException($"{token.Name} get failed");

            return tokenInfo;
        }
    }
}