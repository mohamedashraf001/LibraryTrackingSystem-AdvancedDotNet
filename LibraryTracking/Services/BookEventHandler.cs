using LibraryTracking.DomainEvents;

namespace LibraryTracking.Services;

public class BookEventHandler
{
    // lightweight handler; used if you want to process domain events synchronously
    public Task Handle(BookCreatedEvent evt)
    {
        Console.WriteLine($"[Domain Event] Book created: {evt.BookId} - {evt.Title}");
        return Task.CompletedTask;
    }
}
