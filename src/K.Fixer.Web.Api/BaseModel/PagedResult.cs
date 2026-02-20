namespace K.Fixer.Web.Api.BaseModel;

public record PagedRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip => (PageNumber - 1) * PageSize;
}