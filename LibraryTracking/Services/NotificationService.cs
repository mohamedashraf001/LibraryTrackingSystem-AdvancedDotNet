using Hangfire;
using LibraryTracking.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryTracking.Services;

public class NotificationService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceProvider _provider;

    public NotificationService(IBackgroundJobClient backgroundJobClient, IServiceProvider provider)
    {
        _backgroundJobClient = backgroundJobClient;
        _provider = provider;
    }

    // Enqueue a background job to send notification
    public void EnqueueBookCreatedNotification(int bookId)
    {
        // Enqueue a method on this service type — Hangfire will resolve NotificationService from DI when running
        _backgroundJobClient.Enqueue<NotificationService>(s => s.SendBookCreatedNotification(bookId));
    }

    // The actual method Hangfire will execute (must be public)
    public async Task SendBookCreatedNotification(int bookId)
    {
        // Example: load book and "send notification"
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookId);
        if (book == null)
        {
            Console.WriteLine($"[Notification] Book {bookId} not found.");
            return;
        }

        // Simulate sending notification (email/push)
        Console.WriteLine($"[Notification] Sending notification: Book '{book.Title}' created (Id: {bookId}).");
        // TODO: integrate real email/push provider
    }
}
