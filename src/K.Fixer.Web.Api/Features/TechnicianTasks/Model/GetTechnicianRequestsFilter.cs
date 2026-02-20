using K.Fixer.Web.Api.BaseModel;

namespace K.Fixer.Web.Api.Features.TechnicianTasks.Model;

public record GetTechnicianRequestsFilter : PagedRequest
{
    public string? Status { get; init; }
}
