using Newtonsoft.Json;

namespace Nexplorer.Web.Dtos
{
    public class DataTableSearchDto
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("regex")]
        public string Regex { get; set; }
    }
}