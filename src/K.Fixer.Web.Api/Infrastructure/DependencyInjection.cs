using System.Text.Json.Serialization;

using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Seed;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Web.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterForApp(this IServiceCollection services, IConfiguration configuration)
    {
        Console.WriteLine("APP: RegisterForApp");

        ConfigureJsonSerializerOptions(services);
        ConfigureDatabase(services, configuration);
        services.AddScoped<ISeedService, SeedService>();
        K.Fixer.Infrastructure.Installers.InstallAuth.Install(services);
        K.Fixer.Infrastructure.Installers.InstallPropertyManagement.Install(services);
        K.Fixer.Infrastructure.Installers.InstallCommon.Install(services);
        K.Fixer.Infrastructure.Installers.InstallMaintenance.Install(services);
        
        return services;
    }
    
    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Main");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                // npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "config");
                npgsql.CommandTimeout(30);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

        });
    }

    private static void ConfigureJsonSerializerOptions(IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }
}