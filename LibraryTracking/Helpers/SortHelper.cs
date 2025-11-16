namespace LibraryTracking.Helpers;

public static class SortHelper
{
    // placeholder for client-side or in-memory operations if needed
    public static IEnumerable<T> OrderByProperty<T>(this IEnumerable<T> source, string prop)
    {
        var property = typeof(T).GetProperty(prop);
        if (property == null) return source;
        return source.OrderBy(x => property.GetValue(x, null));
    }
}
