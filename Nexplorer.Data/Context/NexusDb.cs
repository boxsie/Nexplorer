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
        public DbSet<TransactionInput> TransactionInput { get; set; }
        public DbSet<TransactionOutput> TransactionOutput { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AddressAggregate> AddressAggregates { get; set; }
        public DbSet<TrustKey> TrustKeys { get; set; }

        public DbSet<OrphanBlock> OrphanBlocks { get; set; }
        public DbSet<OrphanTransaction> OrphanTransactions { get; set; }

        public DbSet<BittrexSummary> BittrexSummaries { get; set; }

        public NexusDb(DbContextOptions<NexusDb> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Block>();
                //.HasIndex(x => x.Hash);
            modelBuilder.Entity<Block>();
                //.HasIndex(x => x.TimeUtc);
            modelBuilder.Entity<Block>();
                //.HasIndex(x => x.Channel);
            modelBuilder.Entity<Block>()
                .HasMany(x => x.Transactions)
                .WithOne(x => x.Block)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>();
                //.HasIndex(x => x.Hash);
            modelBuilder.Entity<Transaction>();
                //.HasIndex(x => x.TimeUtc);
            modelBuilder.Entity<Transaction>();
                //.HasIndex(x => x.Confirmations);
            modelBuilder.Entity<Transaction>()
                .HasMany(x => x.Inputs)
                .WithOne(x => x.Transaction)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Transaction>()
                .HasMany(x => x.Outputs)
                .WithOne(x => x.Transaction)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransactionInput>();
                //.HasIndex(x => x.Amount);
            modelBuilder.Entity<TransactionInput>()
                .HasOne(x => x.Address)
                .WithMany();

            modelBuilder.Entity<TransactionOutput>();
                //.HasIndex(x => x.Amount);
            modelBuilder.Entity<TransactionOutput>()
                .HasOne(x => x.Address)
                .WithMany();

            modelBuilder.Entity<Address>();
                //.HasIndex(x => x.Hash)
                //.IsUnique();
            modelBuilder.Entity<Address>()
                .HasOne(x => x.FirstBlock)
                .WithMany();

            modelBuilder.Entity<AddressAggregate>();
                //.HasIndex(x => x.Balance);
            modelBuilder.Entity<AddressAggregate>()
                .HasOne(x => x.LastBlock)
                .WithMany();

            modelBuilder.Entity<OrphanBlock>();
                //.HasIndex(x => x.Hash);
            modelBuilder.Entity<OrphanBlock>()
                .HasMany(x => x.Transactions);

            modelBuilder.Entity<OrphanTransaction>();
                //.HasIndex(x => x.Hash);

            modelBuilder.Entity<TrustKey>()
                .HasOne(x => x.Address)
                .WithMany();
            modelBuilder.Entity<TrustKey>()
                .HasOne(x => x.GenesisBlock)
                .WithMany();
            modelBuilder.Entity<TrustKey>()
                .HasOne(x => x.Transaction)
                .WithMany();
        }
    }
}
