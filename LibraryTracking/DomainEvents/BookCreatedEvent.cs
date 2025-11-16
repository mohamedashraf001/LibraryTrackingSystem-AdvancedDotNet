using LibraryTracking.Models;

namespace LibraryTracking.DomainEvents;

public class BookCreatedEvent
{
    public int BookId { get; }
    public string Title { get; }

    public BookCreatedEvent(Book book)
    {
        BookId = book.Id;
        Title = book.Title;
    }
}
