using LibraryTracking.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryTracking.Services;

public class CleanupService
{
    private readonly IServiceProvider _provider;

    public CleanupService(IServiceProvider provider)
    {
        _provider = provider;
    }

    // Remove hard deleted items that were soft-deleted more than `days` days ago.
    public async Task CleanSoftDeletedOlderThanDays(int days)
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoff = DateTime.UtcNow.AddDays(-days);

        // LastUpdated is a shadow property
        var toDelete = await db.Books
            .IgnoreQueryFilters() // to include soft-deleted rows
            .Where(b => EF.Property<bool>(b, "IsDeleted") == true &&
                        EF.Property<DateTime>(b, "LastUpdated") < cutoff)
            .ToListAsync();

        if (!toDelete.Any())
        {
            Console.WriteLine("[Cleanup] No old soft-deleted items to remove.");
            return;
        }

        db.Books.RemoveRange(toDelete);
        await db.SaveChangesAsync();

        Console.WriteLine($"[Cleanup] Permanently deleted {toDelete.Count} soft-deleted books older than {days} days.");
    }
}
