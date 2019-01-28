using Microsoft.EntityFrameworkCore;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Entity.Exchange;
using Nexplorer.Domain.Entity.Orphan;

namespace Nexplorer.Data.Context
{
    public class NexusTestDb : NexusDb
    {
        public NexusTestDb(DbContextOptions<NexusDb> options) : base(options)
        {
        }
    }
}
