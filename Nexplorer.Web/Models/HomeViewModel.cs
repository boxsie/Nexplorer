using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity;

namespace Nexplorer.Web.Models
{
    public class HomeViewModel
    {
        public BlockLiteDto LastBlock { get; set; }
        public double LastPrimeDifficulty { get; set; }
        public double LastHashDifficulty { get; set; }
        public double LastPosDifficulty { get; set; }
    }
}
