namespace K.Fixer.Web.Api.BaseModel;

public record ApiErrorResponse
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public List<string>? ValidationErrors { get; set; }
}