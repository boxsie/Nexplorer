using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Domain.Entity.User;

namespace Nexplorer.Data.Context
{
    public class NexplorerDb : IdentityDbContext<ApplicationUser>
    {
        public DbSet<FavouriteAddress> FavouriteAddresses { get; set; }

        public NexplorerDb(DbContextOptions<NexplorerDb> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.FavouriteAddresses)
                .WithOne(x => x.User)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}