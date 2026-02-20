namespace K.Fixer.Application.Maintenance.GetCompanyMaintenanceRequest;

public record GetCompanyMaintenanceRequestFilter(string? QuerySearch, string? Status, string? Priority, int PageNumber, int PageSize, Guid CompanyId);