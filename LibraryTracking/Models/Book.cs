using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryTracking.Models.ValueObjects;

namespace LibraryTracking.Models;

public class Book
{
    public int Id { get; set; }

    [Required, MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Author { get; set; } = string.Empty;

    // Value Object
    public Price Price { get; set; } = new Price(0m, "USD");

    public bool IsDeleted { get; set; } = false;

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
