using Newtonsoft.Json;

namespace Nexplorer.Web.Dtos
{
    public class DataTableOrderDto
    {
        [JsonProperty("column")]
        public int Column { get; set; }

        [JsonProperty("dir")]
        public string Dir { get; set; }
    }
}