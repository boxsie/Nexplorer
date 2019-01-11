using System.Collections.Generic;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Domain.Criteria
{
    public class FilterResult<T>
    {
        public int ResultCount { get; set; }
        public List<T> Results { get; set; }
    }
}