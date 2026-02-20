using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Seed.FixedValue;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Web.Seed.ShowcaseData;


public class BuildingSeeder : ISeeder
{
    private readonly AppDbContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BuildingSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        await SeedAcmeBuildingsAsync();
        await SeedCompanyBuildingsAsync(CompanyGuids.GlobalTech, "Shire Road", 4, 67891);
        await SeedCompanyBuildingsAsync(CompanyGuids.NovaSoft, "Innovation Ave", 3, 54321);
        await SeedCompanyBuildingsAsync(CompanyGuids.IronForge, "Industrial Blvd", 6, 98765);
        await _dbContext.SaveChangesAsync();
    }

    private async ValueTask SeedAcmeBuildingsAsync()
    {
        var companyId = await _dbContext.CompaniesReads
            .Where(c => c.Id == CompanyGuids.Acme)
            .Select(c => c.Id).FirstAsync();

        var fixedBuildings = new List<(Guid Gid, string Name, string City, string street, string postalCode)>
        {
            (BuildingGuids.AcmeMainOffice, "Main Office", "Warsaw", "Acme Plaza 1", "44-555"),
            (BuildingGuids.AcmeWarehouseA, "Warehouse A", "Łódź", "Logistics Park 12", "44-333"),
            (BuildingGuids.AcmeWarehouseB, "Warehouse B", "Łódź", "Logistics Park 32", "44-333"),
            (BuildingGuids.AcmeTechCenter, "Tech Center", "Kraków", "5 Innovation Blvd", "99-009"),
            (BuildingGuids.AcmeSalesHub, "Sales Hub", "Gdańsk", "78 Commerce St, ", "33-449"),
        };

        foreach (var (gid, name, city, street, postalCode) in fixedBuildings)
        {
            if (await _dbContext.Buildings.AnyAsync(b => b.Id == gid))
            {
                continue;
            }

            _dbContext.Buildings.Add(Building.Create(companyId, name, street, city, postalCode, "PL", gid).Value!);
        }
    }

    private async ValueTask SeedCompanyBuildingsAsync(Guid companyGid, string streetBase, int count, int randomSeed)
    {


        var existingCount = await _dbContext.Buildings.CountAsync(b => b.CompanyId == companyGid);
        if (existingCount >= count) return;

        var random = new Random(randomSeed);

        var buildingNames = new[]
        {
            "Main Office", "Branch Office", "Warehouse", "Tech Hub", "Operations Center", "Regional HQ",
            "Support Center", "Distribution Center", "Research Lab", "Sales Office"
        };

        var cities = new[] { "Warsaw", "Kraków", "Wrocław", "Gdańsk", "Poznań", "Łódź" };

        var existingNames = await _dbContext.Buildings
            .Where(b => b.CompanyId == companyGid)
            .Select(b => b.Name.Value)
            .ToHashSetAsync();

        for (var i = 0; i < count; i++)
        {
            var name = buildingNames[i % buildingNames.Length];
            if (!existingNames.Add(name))
                name = $"{name} {i + 1}";

            var city = cities[random.Next(cities.Length)];
            var number = random.Next(1, 200);

            _dbContext.Buildings.Add(Building.Create(companyGid, name, "Some street " + number, city, "35-444", "PL").Value!);
        }
    }
}