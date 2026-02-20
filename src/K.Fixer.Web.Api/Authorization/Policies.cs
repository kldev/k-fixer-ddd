namespace K.Fixer.Web.Api.Authorization;

public static class Policies
{
    public const string CanManageCompany = nameof(CanManageCompany);
    public const string CanCreateBuilding = nameof(CanCreateBuilding);    // Admin only
    public const string CanManageOwnBuilding = nameof(CanManageOwnBuilding); // CompanyAdmin own company
    public const string CanCreateMaintenanceRequest = nameof(CanCreateMaintenanceRequest);
    public const string CanManageCompanyRequests = nameof(CanManageCompanyRequests);
    public const string CanManageTechnicianTasks = nameof(CanManageTechnicianTasks);
}