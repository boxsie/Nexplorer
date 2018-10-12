using Newtonsoft.Json;

namespace Nexplorer.Web.Dtos
{
    public class DataTableColumnDto
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("orderable")]
        public bool Orderable { get; set; }

        [JsonProperty("search")]
        public DataTableSearchDto Search { get; set; }
    }
}