using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Models;

namespace Nexplorer.Web.Models
{
    public class TransactionItemViewModel
    {
        public TransactionModel Transaction { get; set; }
        public bool ShowHash { get; set; }
    }
}
