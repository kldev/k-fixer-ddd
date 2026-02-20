using K.Fixer.Application.Events;
using K.Fixer.Infrastructure.Events;

using Microsoft.Extensions.DependencyInjection;

namespace K.Fixer.Infrastructure.Installers;

public static class InstallCommon
{
    public static IServiceCollection Install(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}