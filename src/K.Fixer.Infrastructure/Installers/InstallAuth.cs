using K.Fixer.Application.IAM.AdminLogin;
using K.Fixer.Application.IAM.Login;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Infrastructure.Auth;
using K.Fixer.Infrastructure.Auth.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace K.Fixer.Infrastructure.Installers;

public static class InstallAuth
{
    public static IServiceCollection Install(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<AdminLoginHandler>();
        
        return services;
    } 
}