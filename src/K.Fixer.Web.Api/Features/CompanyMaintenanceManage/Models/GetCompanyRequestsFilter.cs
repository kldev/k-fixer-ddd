using K.Fixer.Web.Api.BaseModel;

namespace K.Fixer.Web.Api.Features.CompanyMaintenanceManage.Models;

public record GetCompanyRequestsFilter : PagedRequest
{
    public string? QuerySearch { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
}
