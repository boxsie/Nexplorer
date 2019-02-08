using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Models;

namespace Nexplorer.Web.Models
{
    public class TransactionViewModel
    {
        public TransactionDto Transaction { get; set; }
        public string BlockHash { get; set; }
    }
}