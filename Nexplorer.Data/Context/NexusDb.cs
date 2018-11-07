using Microsoft.EntityFrameworkCore;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Entity.Exchange;
using Nexplorer.Domain.Entity.Orphan;

namespace Nexplorer.Data.Context
{
    public class NexusDb : DbContext
    {
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionInputOutput> TransactionInputOutput { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AddressAggregate> AddressAggregates { get; set; }
        public DbSet<TrustKey> TrustKeys { get; set; }

        public DbSet<OrphanBlock> OrphanBlocks { get; set; }
        public DbSet<OrphanTransaction> OrphanTransactions { get; set; }

        public DbSet<BittrexSummary> BittrexSummaries { get; set; }

        public NexusDb(DbContextOptions<NexusDb> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Indexes

            //modelBuilder.Entity<Block>()
            //    .HasIndex(x => x.Hash);

            //modelBuilder.Entity<Transaction>()
            //    .HasIndex(x => x.Hash);

            //modelBuilder.Entity<Address>()
            //    .HasIndex(x => x.Hash);

            //modelBuilder.Entity<AddressAggregate>()
            //    .HasIndex(x => x.Balance);


            // Relationships

            modelBuilder.Entity<TransactionInputOutput>()
                .HasOne(x => x.Transaction)
                .WithMany(x => x.InputOutputs)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AddressAggregate>()
                .HasOne(x => x.LastBlock)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrustKey>()
                .HasOne(x => x.GenesisBlock)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TrustKey>()
                .HasOne(x => x.Transaction)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TrustKey>()
                .HasOne(x => x.Address)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
