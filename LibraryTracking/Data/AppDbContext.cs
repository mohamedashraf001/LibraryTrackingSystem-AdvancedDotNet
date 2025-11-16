using LibraryTracking.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryTracking.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft delete global filter
        modelBuilder.Entity<Book>().HasQueryFilter(b => !b.IsDeleted);

        // RowVersion concurrency
        modelBuilder.Entity<Book>()
            .Property(b => b.RowVersion)
            .IsRowVersion();

        // Shadow property LastUpdated
        modelBuilder.Entity<Book>()
            .Property<DateTime>("LastUpdated")
            .HasDefaultValueSql("GETUTCDATE()");

        // Map ValueObject Price as owned
        modelBuilder.Entity<Book>()
            .OwnsOne(b => b.Price, p =>
            {
                p.Property(x => x.Amount).HasColumnName("Price").HasPrecision(18, 2);
                p.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(5);
            });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Book>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            entry.Property("LastUpdated").CurrentValue = now;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
