using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Domain.Shared;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Seed.FixedValue;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Web.Seed.ShowcaseData;

public class ResidentBuildingSeeder : ISeeder
{
    private static readonly Random _rand = new();
    private const string DefaultPassword = "fixer777";

    // One resident per fixed Acme building; daniel@cme.com may already exist from UserSeeder
    private static readonly (Guid BuildingId, string Email, string FullName)[] AcmeBuildingAssignments =
    [
        (BuildingGuids.AcmeMainOffice, "daniel@acme.com",  "Daniel Silver"),
        (BuildingGuids.AcmeWarehouseA, "m.green@acme.com", "Michael Green"),
        (BuildingGuids.AcmeWarehouseB, "s.white@acme.com", "Sarah White"),
        (BuildingGuids.AcmeTechCenter, "r.black@acme.com", "Robert Black"),
        (BuildingGuids.AcmeSalesHub,   "e.brown@acme.com", "Emma Brown"),
    ];

    private readonly AppDbContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ResidentBuildingSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        foreach (var (index, (buildingId, email, fullName)) in AcmeBuildingAssignments.Select((v, i) => (i, v)))
        {
            var building = await _dbContext.Set<Building>()
                .Include(b => b.Occupancies)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building is null)
                continue;

            // Skip if this building already has an active occupancy
            if (building.Occupancies.Any(o => o.IsActive()))
                continue;

            var user = await _dbContext.Users.FirstOrDefaultAsync(z => z.Email == Email.Create(email).Value);
            if (user == null)
            {
                var createResult = User.Create(email, DefaultPassword, fullName, UserRole.Resident.Value,
                    CompanyGuids.Acme);
                if (createResult.IsFailure)
                    continue;

                user = createResult.Value!;
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }

            // First building (AcmeMainOffice) gets 2 ended assignments before the active one
            if (index == 0)
            {
                building.AssignResident(user.Id, DateTime.UtcNow.AddDays(-120));
                building.RemoveResident(user.Id, DateTime.UtcNow.AddDays(-80));
                building.AssignResident(user.Id, DateTime.UtcNow.AddDays(-70));
                building.RemoveResident(user.Id, DateTime.UtcNow.AddDays(-15));
            }

            building.AssignResident(user.Id, DateTime.UtcNow.AddDays(-_rand.Next(10, 99)));

            var entity = MaintenanceRequest.Create(buildingId, user.Id, "Test request #" + index,
                "The request description #" + index).Value!;
            await _dbContext.Requests.AddAsync(entity);
        }

        await _dbContext.SaveChangesAsync();
    }


}
