using LibraryTracking.Data;
using LibraryTracking.Data.Queries;
using LibraryTracking.Models;
using LibraryTracking.Models.Dtos;
using LibraryTracking.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryTracking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly BookQueries _queries;
    private readonly NotificationService _notificationService;

    public BooksController(AppDbContext db, BookQueries queries, NotificationService notificationService)
    {
        _db = db;
        _queries = queries;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get books with filtering, sorting and pagination.
    /// Query params:
    /// - page (1-based)
    /// - pageSize
    /// - sortBy (Title, Author, Price)
    /// - sortDir (asc|desc)
    /// - title (contains)
    /// - author (contains)
    /// - minPrice
    /// - maxPrice
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] BookFilter filter)
    {
        var paged = await _queries.GetPagedAsync(filter);
        return Ok(paged);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _queries.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
    {
        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            Price = new Models.ValueObjects.Price(dto.Price, dto.Currency)
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        // enqueue background notification job using Hangfire inside NotificationService
        _notificationService.EnqueueBookCreatedNotification(book.Id);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto)
    {
        var existing = await _db.Books.FindAsync(id);
        if (existing is null) return NotFound();

        // Concurrency: client can send RowVersion if desired (not shown here)
        existing.Title = dto.Title;
        existing.Author = dto.Author;
        existing.Price = new Models.ValueObjects.Price(dto.Price, dto.Currency);

        try
        {
            await _db.SaveChangesAsync();
            return Ok(existing);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            return Conflict("Concurrency conflict detected. The record was modified by another process.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return NotFound();

        book.IsDeleted = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Book soft-deleted" });
    }
}
