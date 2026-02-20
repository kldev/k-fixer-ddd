namespace K.Fixer.Application.Maintenance.GetTechnicianRequests;

public sealed record GetTechnicianRequestsFilter(Guid? TechnicianId, string? Status, int PageNumber, int PageSize);