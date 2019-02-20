using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Dtos
{
    public class DataTablePostModel<T>
    {
        [JsonProperty("filterCriteria")]
        public T FilterCriteria { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }
    }
}
