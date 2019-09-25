using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Accounts.Models;
using Nexplorer.Nexus.Enums;
using Nexplorer.Nexus.Extensions;
using Nexplorer.Nexus.Ledger.Models;
using Nexplorer.Nexus.Nexus;

namespace Nexplorer.Nexus.Accounts
{
    public class AccountService : NexusService
    {
        public AccountService(NexusNode node, ILogger<NexusService> log) : base(node, log) { }

        public async Task<GenesisId> CreateAccountAsync(NexusUserCredential userCredential, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            userCredential.Validate();

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"username", userCredential.Username},
                {"password", userCredential.Password},
                {"pin", userCredential.Pin.ToString()}
            });
            
            var tx = await PostAsync<Transaction>("accounts/create", request, token);

            if (string.IsNullOrWhiteSpace(tx?.Genesis))
                throw new InvalidOperationException($"Create account {userCredential.Username} failed");

            return new GenesisId {Genesis = tx.Genesis};
        }

        public async Task<NexusUser> LoginAsync(NexusUserCredential userCredential, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            userCredential.Validate();

            var param = new Dictionary<string, string>
            {
                {"username", userCredential.Username},
                {"password", userCredential.Password}
            };

            if (userCredential.Pin.HasValue)
                param.Add("pin", userCredential.Pin.Value.ToString());

            var genesisId = await PostAsync<GenesisId>("accounts/login", new NexusRequest(param), token);

            if (string.IsNullOrWhiteSpace(genesisId?.Session))
                throw new InvalidOperationException($"{userCredential.Username} login failed");
            
            return new NexusUser
            {
                GenesisId = genesisId,
                Pin = userCredential.Pin
            };
        }

        public async Task<GenesisId> LogoutAsync(string sessionId = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var request = !string.IsNullOrWhiteSpace(sessionId)
                ? new NexusRequest(new Dictionary<string, string> {{"session", sessionId}})
                : null;

            var id = await PostAsync<GenesisId>("accounts/logout", request, token);

            if (string.IsNullOrWhiteSpace(id?.Genesis))
                throw new InvalidOperationException("Logout failed");

            return id;
        }

        public async Task<bool> LockAsync(string sessionId, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (ApiSessions)
                throw new InvalidOperationException("Cannot lock with API sessions enabled");

            var request = new NexusRequest(new Dictionary<string, string> {{"session", sessionId}} );

            return await PostAsync<bool>("accounts/lock", request, token);
        }

        public async Task<bool> UnlockAsync(int pin, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (ApiSessions)
                throw new InvalidOperationException("Cannot unlock with API sessions enabled");

            if (pin.ToString().Length < 4)
                throw new ArgumentException("A valid PIN is required to unlock the account");

            var request = new NexusRequest(new Dictionary<string, string> {{"pin", pin.ToString()}});

            return await PostAsync<bool>("accounts/unlock", request, token);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(GenesisId genesis, 
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(genesis?.Genesis))
                throw new ArgumentException("Genesis is required");

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"genesis", genesis.Genesis},
                {"verbose", ((int)txVerbosity).ToString()}
            });

            var txs = await PostAsync<IEnumerable<Transaction>>("accounts/transactions", request, token);

            return txs ?? new List<Transaction>();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(string userName,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username is required");

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"username", userName},
                {"verbose", ((int)txVerbosity).ToString()}
            });

            var txs = await PostAsync<IEnumerable<Transaction>>("accounts/transactions", request, token);

            return txs ?? new List<Transaction>();
        }

        public async Task<IEnumerable<Transaction>> GetNotificationsAsync(string userName,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username is required");

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"username", userName},
                {"verbose", ((int)txVerbosity).ToString()}
            });

            var txs = await PostAsync<IEnumerable<Transaction>>("accounts/notifications", request, token);

            return txs ?? new List<Transaction>();
        }

        public async Task<IEnumerable<Transaction>> GetNotificationsAsync(GenesisId genesis,
            TxVerbosity txVerbosity = TxVerbosity.PubKeySign, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(genesis?.Genesis))
                throw new ArgumentException("Genesis is required");

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"genesis", genesis.Genesis},
                {"verbose", ((int)txVerbosity).ToString()}
            });

            var txs = await PostAsync<IEnumerable<Transaction>>("accounts/notifications", request, token);

            return txs ?? new List<Transaction>();
        }
    }
}