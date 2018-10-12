using System.Collections.Generic;

namespace Nexplorer.Data.Api
{
    public class FilterResponse<T, TY> where TY : FilterCriteria
    {
        public int FilterResultCount { get; set; }
        public TY Criteria { get; set; }
        public List<T> Data { get; set; }
    }
}