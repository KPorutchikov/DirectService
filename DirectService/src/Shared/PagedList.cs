namespace Shared;

public class PagedList<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = [];
    
    public long TotalCount { get; init; }
    
    public int PageSize { get; init; }
    
    public int Page { get; init; }
}