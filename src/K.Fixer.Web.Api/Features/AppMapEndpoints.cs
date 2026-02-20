namespace K.Fixer.Web.Api.Features;

public static class AppMapEndpoints
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Pong!")
            .AllowAnonymous()
            .ExcludeFromDescription();
        
        Auth.Endpoint.Map(app);
        CompanyManage.Endpoint.Map(app);
        CompanyBuildings.Endpoint.Map(app);
        ResidentRequests.Endpoint.Map(app);
        CompanyMaintenanceManage.Endpoint.Map(app);
        TechnicianTasks.Endpoint.Map(app);
    }
}