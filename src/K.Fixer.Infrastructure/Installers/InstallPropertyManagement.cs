using K.Fixer.Application.PropertyManagement.CreateCompany;
using K.Fixer.Application.PropertyManagement.CreateCompanyBuilding;
using K.Fixer.Application.PropertyManagement.GetCompanies;
using K.Fixer.Application.PropertyManagement.GetCompanyBuildings;
using K.Fixer.Application.PropertyManagement.UpdateCompany;
using K.Fixer.Application.PropertyManagement.UpdateCompanyBuilding;
using K.Fixer.Domain.PropertyManagement.Repositories;
using K.Fixer.Infrastructure.PropertyManagement.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace K.Fixer.Infrastructure.Installers;

public static class InstallPropertyManagement
{
    public static IServiceCollection Install(this IServiceCollection services)
    {
  
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyBuildingRepository, CompanyBuildingRepository>();
        services.AddScoped<CreateCompanyHandler>();
        services.AddScoped<UpdateCompanyHandler>();
        services.AddScoped<CreateCompanyBuildingHandler>();
        services.AddScoped<UpdateCompanyBuildingHandler>();
        services.AddScoped<GetCompaniesHandler>();
        services.AddScoped<GetCompanyBuildingsHandler>();
        
        return services;
    } 
}