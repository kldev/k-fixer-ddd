using K.Fixer.Web.Api.Authorization;
using K.Fixer.Web.Api.Extensions;
using K.Fixer.Web.Api.Features;
using K.Fixer.Web.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.RegisterForApp(builder.Configuration);

    builder.Services.AddJwtBearer(builder.Configuration);
    builder.Services.BuildPolicies();
    
    builder.Services.AddOpenApi();
    builder.Services.SetupSwaggerForApp();
}

var app = builder.Build();
{
    app.UseAuthentication();
    app.UseAuthorization();

    // Database migration on startup
    await app.InitializeDatabaseAsync();
    app.MapOpenApi();
    app.MapAppEndpoints();
    app.UseSwaggerForApp();
    app.Run();
}

// Required for WebApplicationFactory in tests
public partial class Program;
