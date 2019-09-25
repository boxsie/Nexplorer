using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexplorer.Nexus.Extensions;
using HttpMethod = Nexplorer.Nexus.Enums.HttpMethod;

namespace Nexplorer.Nexus.Nexus
{
    public abstract class NexusService
    {
        public bool ApiSessions { get; set; }
        public bool IndexedHeight { get; set; }

        private readonly ILogger<NexusService> _log;
        private readonly INexusClient _client;
        private readonly JsonSerializerSettings _settings;

        protected NexusService(NexusNode node, ILogger<NexusService> log)
        {
            ApiSessions = node.Endpoint.ApiSessions;
            IndexedHeight = node.Endpoint.IndexHeight;

            _client = node.Client;

            _log = log;

            _settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new LowercaseContractResolver()
            };
        }

        protected Task<T> GetAsync<T>(string path, NexusRequest request, CancellationToken token = default, bool logOutput = true)
        {
            return RequestAsync<T>(HttpMethod.Get, path, request, token, logOutput);
        }

        protected Task<T> PostAsync<T>(string path, NexusRequest request, CancellationToken token = default, bool logOutput = true)
        {
            return RequestAsync<T>(HttpMethod.Post, path, request, token, logOutput);
        }

        private async Task<T> RequestAsync<T>(HttpMethod httpMethod, string path, NexusRequest request, CancellationToken token, bool logOutput)
        {
            token.ThrowIfCancellationRequested();

            var requestName = typeof(T).Name;
            var logHeader = $"API {(httpMethod == HttpMethod.Get ? "GET" : "POST")} {path}:";

            try
            {
                if (path == null)
                {
                    _log.LogError($"Path is missing for '{requestName}' get request");
                    return default;
                }

                if (path[0] == '/')
                    path = path.Remove(0, 1);

                var response = httpMethod == HttpMethod.Get
                    ? await _client.GetAsync(path, logHeader, request, token, logOutput) 
                    : await _client.PostAsync(path, logHeader, request, token, logOutput);

                var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<NexusResponse<T>>(responseJson, _settings);

                if (response.IsSuccessStatusCode)
                {
                    _log.LogInformation($"{logHeader} SUCCESS");

                    if (logOutput)
                        _log.LogInformation($"{logHeader} {JsonConvert.SerializeObject(result.Result)}");

                    return result.Result;
                }

                if (!logOutput)
                    return default;

                _log.LogError($"{logHeader} FAILED");
                _log.LogError($"{logHeader} {response.StatusCode} {(result.Error != null ? $"From Nexus->'{result.Error.Code} - {result.Error.Message}'" : responseJson)}");

                return default;
            }
            catch (Exception e)
            {
                if (!logOutput)
                    return default;

                _log.LogError($"{logHeader} FAILED");
                _log.LogError(e.Message);
                if (e.InnerException != null)
                    _log.LogError(e.InnerException.Message);
                _log.LogError(e.StackTrace);

                return default;
            }
        }
    }
}