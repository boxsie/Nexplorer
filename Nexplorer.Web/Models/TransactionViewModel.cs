using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Models;

namespace Nexplorer.Web.Models
{
    public class TransactionViewModel
    {
        public TransactionModel Transaction { get; set; }
        public string BlockHash { get; set; }
        public int MaxConfirmations { get; set; }
    }
}