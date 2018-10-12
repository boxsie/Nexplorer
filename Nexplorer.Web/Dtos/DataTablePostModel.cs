using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Dtos
{
    public class DataTablePostModel
    {
        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("filterDto")]
        public AddressFilterCriteria FilterCriteria { get; set; }

        [JsonProperty("draw")]
        public int Draw { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("columns")]
        public List<DataTableColumnDto> Columns { get; set; }

        [JsonProperty("search")]
        public DataTableSearchDto Search { get; set; }

        [JsonProperty("order")]
        public List<DataTableOrderDto> Order { get; set; }
    }
}
