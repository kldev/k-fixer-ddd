namespace K.Fixer.Application.Common;


public record PagedResult<T>(IEnumerable<T> Items,  int TotalCount, int PageNumber,int PageSize  )
{ 
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
