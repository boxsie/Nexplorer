using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexplorer.Nexus.Extensions;

namespace Nexplorer.Nexus
{
    public class NexusClient : INexusClient
    {
        public NexusNodeEndpoint NexusEndpoint { get; }

        private readonly HttpClient _client;
        private readonly ILogger<NexusClient> _log;
        private readonly JsonSerializerSettings _settings;

        public NexusClient(HttpClient client, NexusNodeEndpoint endpoint, ILogger<NexusClient> log)
        {
            NexusEndpoint = endpoint;

            _client = client;
            _log = log;

            _settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new LowercaseContractResolver()
            };

            ConfigureHttpClient(endpoint);
        }

        public Task<NexusResponse<T>> GetAsync<T>(string path, NexusRequest request, CancellationToken token = default)
        {
            return RequestAsync<T>(HttpMethod.Get, path, request, token);
        }

        public Task<NexusResponse<T>> PostAsync<T>(string path, NexusRequest request, CancellationToken token = default)
        {
            return RequestAsync<T>(HttpMethod.Post, path, request, token);
        }

        private void ConfigureHttpClient(NexusNodeEndpoint endpoint)
        {
            if (_client == null)
                throw new NullReferenceException("Http client is null");

            if (endpoint == null)
                throw new ArgumentException("Parameters are missing");

            if (string.IsNullOrWhiteSpace(endpoint.Url))
                throw new ArgumentException("URL is missing");

            if (string.IsNullOrWhiteSpace(endpoint.Username))
                throw new ArgumentException("Username is missing");

            if (string.IsNullOrWhiteSpace(endpoint.Password))
                throw new ArgumentException("Password is missing");

            var uriResult = Uri.TryCreate(endpoint.Url, UriKind.Absolute, out var baseUri)
                            && (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps);

            if (!uriResult)
                throw new Exception("Url is not valid");

            var auth64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{endpoint.Username}:{endpoint.Password}"));

            _client.BaseAddress = baseUri;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth64);
            _client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
        }

        private async Task<NexusResponse<T>> RequestAsync<T>(HttpMethod httpMethod, string path, NexusRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var requestName = typeof(T).Name;
            var logHeader = $"API {(httpMethod == HttpMethod.Get ? "GET" : "POST")} {path}:";
            
            string responseJson;
            HttpResponseMessage response;

            try
            {
                if (path == null)
                {
                    _log.LogError($"Path is missing for '{requestName}' get request");
                    return default;
                }

                if (path[0] == '/')
                    path = path.Remove(0, 1);

                response = httpMethod == HttpMethod.Get
                    ? await GetAsync(path, logHeader, request, token)
                    : await PostAsync(path, logHeader, request, token);

                responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.LogError($"{logHeader} FAILED");
                _log.LogError(e.Message);

                if (e.InnerException != null)
                    _log.LogError(e.InnerException.Message);

                _log.LogError(e.StackTrace);

                return new NexusResponse<T> { CanConnect = false, Error = new NexusError { Message = e.Message } };
            }

            try
            { 
                var result = JsonConvert.DeserializeObject<NexusResponse<T>>(responseJson, _settings);

                if (response.IsSuccessStatusCode)
                {
                    _log.LogInformation($"{logHeader} SUCCESS");
                    _log.LogTrace($"{logHeader} {JsonConvert.SerializeObject(result.Result)}");

                    return result;
                }

                _log.LogError($"{logHeader} FAILED");
                _log.LogError($"{logHeader} {response.StatusCode} {(result.Error != null ? $"From Nexus->'{result.Error.Code} - {result.Error.Message}'" : responseJson)}");
                result.IsNodeError = true;

                return result;
            }
            catch (Exception e)
            {
                _log.LogError($"{logHeader} FAILED");
                _log.LogError(e.Message);
                if (e.InnerException != null)
                    _log.LogError(e.InnerException.Message);
                _log.LogError(e.StackTrace);

                return new NexusResponse<T> { IsNodeError = true, Error = new NexusError { Message = e.Message }};
            }
        }

        private async Task<HttpResponseMessage> GetAsync(string path, string logHeader, NexusRequest request, CancellationToken token)
        {
            var getRequest = request != null
                ? $"{path}?{request.GetParamString()}"
                : path;
            
            _log.LogInformation($"{logHeader} {getRequest}");

            return await _client.GetAsync(getRequest, token);
        }

        private async Task<HttpResponseMessage> PostAsync(string path, string logHeader, NexusRequest request, CancellationToken token)
        {
            var form = new FormUrlEncodedContent(request?.Param);
            
            _log.LogInformation($"{logHeader} {await form.ReadAsStringAsync()}");

            return await _client.PostAsync(path, form, token).ConfigureAwait(false);
        }
    }
}