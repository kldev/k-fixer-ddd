using K.Fixer.Domain.PropertyManagement.Aggregates.Company;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Seed.FixedValue;
using K.Fixer.Web.Seed.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace K.Fixer.Web.Seed.ShowcaseData;


public class CompanySeeder : ISeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SeedService> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompanySeeder(AppDbContext dbContext, ILogger<SeedService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedFixedCompaniesAsync();
        await SeedRandom150CompaniesAsync();
        await _dbContext.SaveChangesAsync();
    }

    private async ValueTask SeedFixedCompaniesAsync()
    {
        var fixedCompanies = new List<(Guid Id, string Name, string TaxId, string Email, string Phone)>
        {
            (CompanyGuids.Acme, "Acme Corporation", "5667138176", "contact@acme.com", "+48100000001"),
            (CompanyGuids.GlobalTech, "GlobalTech Solutions", "8385853805", "info@globaltech.com", "+48 100 000 002"),
            (CompanyGuids.NovaSoft, "NovaSoft Systems", "7765864269", "hello@novasoft.com", "+48 100 000 003"),
            (CompanyGuids.SkyLine, "SkyLine Industries", "7743924395", "office@skyline.com", "+48 100 000 004"),
            (CompanyGuids.BlueWave, "BlueWave Logistics", "1145478801", "contact@bluewave.com", "+48 100 000 005"),
            (CompanyGuids.IronForge, "IronForge Manufacturing", "8378031835", "info@ironforge.com", "+48 100 000 006"),
            (CompanyGuids.PineCrest, "PineCrest Consulting", "7760609489", "office@pinecrest.com", "+48 100 000 007"),
            (CompanyGuids.RedStone, "RedStone Energy", "1574872148", "contact@redstone.com", "+48 100 000 008"),
            (CompanyGuids.SilverPeak, "SilverPeak Finance", "7629530832", "info@silverpeak.com", "+48 100 000 009"),
            (CompanyGuids.CedarPoint, "CedarPoint Media", "6014573855", "hello@cedarpoint.com", "+48 100 000 010"),
        };

        foreach (var (guid, name, taxId, email, phone) in fixedCompanies)
        {

            if (await _dbContext.Companies.AnyAsync(c => c.Id == guid))
                continue;
            var result = Company.Create(name, "PL", taxId, email, phone, guid);
            if (result.IsSuccess)
            {
                _dbContext.Companies.Add(result.Value!);
            }
            else
            {
                _logger.LogInformation($"Company not created successfully: {name} {result.Error}");
            }
        }
    }

    private async ValueTask SeedRandom150CompaniesAsync()
    {
        var existingCount = await _dbContext.Companies.CountAsync();
        if (existingCount >= 160) return;

        var random = new Random(12345); // fixed seed for reproducibility

        var prefixes = new[]
        {
        "Alpha", "Beta", "Delta", "Sigma", "Omega", "Vertex", "Apex",
        "Prime", "Nova", "Titan", "Orion", "Atlas", "Zenith", "Vortex",
        "Stellar", "Fusion", "Quantum", "Nexus", "Pulse", "Helix",
        "Cobalt", "Ember", "Frost", "Gale", "Harbor", "Ignite", "Jade",
        "Krypton", "Lunar", "Magnum", "Neon", "Obsidian", "Phoenix", "Quasar",
        "Radiant", "Solar", "Thunder", "Ultra", "Vapor", "Wolff"
      };

        var suffixes = new[]
        {
        "Systems", "Solutions", "Technologies", "Group", "Services",
        "Industries", "Dynamics", "Labs", "Ventures", "Partners",
        "Corp", "Digital", "Engineering", "Consulting", "Logistics",
        "Analytics", "Capital", "Collective", "Creations", "Enterprises",
        "Forge", "Global", "Holdings", "Innovations", "Interactive",
        "Media", "Networks", "Operations", "Platforms", "Studio"
      };

        var domains = new[] { "com", "pl", "eu", "io", "net" };

        var existingNames = await _dbContext.Companies.Select(c => c.Name.Value).ToHashSetAsync();
        var existingNips = await _dbContext.Companies.Select(c => c.TaxId.Number).ToHashSetAsync();

        for (var i = 0; i < 150; i++)
        {
            var prefix = prefixes[random.Next(prefixes.Length)];
            var suffix = suffixes[random.Next(suffixes.Length)];
            var name = $"{prefix} {suffix}";
            var taxId = NipHelper.GenerateValidNip(random);
            if (!existingNames.Add(name))
                continue;
            
            if (!existingNips.Add(taxId))
                continue;
            
            var domain = domains[random.Next(domains.Length)];
            var email = $"contact@{prefix.ToLower()}{suffix.ToLower()}.{domain}";
            var hasPhone = random.Next(100) < 60;
            var phoneValue = hasPhone ?  $"+48{random.Next(100, 999)}{random.Next(100, 999)}{random.Next(100, 999)}" : "";

            var result = Company.Create(name, "PL", taxId, email, hasPhone ? phoneValue : "");
            if (result.IsSuccess)
            {
                _dbContext.Companies.Add(result.Value!);
            }
            else
            {
                _logger.LogInformation($"Company not created successfully: {name} {result.Error}");
            }
        }
    }
}