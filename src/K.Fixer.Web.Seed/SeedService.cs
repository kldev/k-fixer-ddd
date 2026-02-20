using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Seed.ShowcaseData;

using Microsoft.Extensions.Logging;

namespace K.Fixer.Web.Seed;

public class SeedService : ISeedService
{
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<SeedService> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SeedService(AppDbContext appDbContext, ILogger<SeedService> logger)
    {
        _appDbContext = appDbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await new BaseDictSeeder(_appDbContext).SeedAsync();
    }

    public async Task SeedShowcaseAsync()
    {
         await new CompanySeeder(_appDbContext, _logger).SeedAsync();
         await new UserSeeder(_appDbContext, _logger).SeedAsync();
         await new BuildingSeeder(_appDbContext).SeedAsync();
         await new ResidentBuildingSeeder(_appDbContext).SeedAsync();
    }
}