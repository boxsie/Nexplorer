using Newtonsoft.Json;

namespace Nexplorer.Infrastructure.Core
{
    public class ExchangeResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }
    }
}