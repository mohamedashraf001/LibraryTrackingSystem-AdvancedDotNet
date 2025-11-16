namespace LibraryTracking.Models.Dtos;

public class BookFilter
{
    // paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // sorting
    public string? SortBy { get; set; } = "Title";
    public string? SortDir { get; set; } = "asc";

    // filtering
    public string? Title { get; set; }
    public string? Author { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
