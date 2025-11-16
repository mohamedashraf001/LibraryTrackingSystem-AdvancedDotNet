using LibraryTracking.Helpers;
using LibraryTracking.Models;
using LibraryTracking.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LibraryTracking.Data.Queries;

public class BookQueries
{
    private readonly AppDbContext _db;

    public BookQueries(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Book>> GetPagedAsync(BookFilter filter)
    {
        // Base query (Global Query Filter hides soft deleted)
        var q = _db.Books.AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            q = q.Where(b => b.Title.Contains(filter.Title));
        }

        if (!string.IsNullOrWhiteSpace(filter.Author))
        {
            q = q.Where(b => b.Author.Contains(filter.Author));
        }

        if (filter.MinPrice.HasValue)
        {
            q = q.Where(b => b.Price.Amount >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            q = q.Where(b => b.Price.Amount <= filter.MaxPrice.Value);
        }

        // Total count before paging
        var total = await q.CountAsync();

        // Sorting
        bool descending = string.Equals(filter.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        q = ApplySorting(q, filter.SortBy ?? "Title", descending);

        // Paging
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Book>(items, total, page, pageSize);
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    private IQueryable<Book> ApplySorting(IQueryable<Book> source, string sortBy, bool desc)
    {
        // support Title, Author, Price
        sortBy = sortBy?.Trim() ?? "Title";
        if (string.Equals(sortBy, "Price", StringComparison.OrdinalIgnoreCase))
        {
            if (desc)
                return source.OrderByDescending(b => b.Price.Amount);
            return source.OrderBy(b => b.Price.Amount);
        }

        // default use reflection on scalar properties (Title/Author)
        var prop = typeof(Book).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (prop == null)
        {
            // fallback
            prop = typeof(Book).GetProperty("Title");
        }

        return desc
            ? source.OrderByDescending(x => EF.Property<object>(x, prop!.Name))
            : source.OrderBy(x => EF.Property<object>(x, prop!.Name));
    }
}
