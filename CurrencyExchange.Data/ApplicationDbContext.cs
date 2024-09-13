using CurrencyExchange.Data.Abstractions;
using CurrencyExchange.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Data
{
    public sealed class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions options)
            : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Entity>().Where(x => x.State == EntityState.Added
            || x.State == EntityState.Modified))
            {
                entry.Entity.DateModified = DateTime.UtcNow;
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.DateCreated = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
        public DbSet<Currency> Currencies { get; set; }
    }
}
