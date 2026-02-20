using K.Fixer.Infrastructure.Maintenance.Repositories;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Api.Tests.Infrastructure;
using K.Fixer.Web.Seed.FixedValue;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Web.Api.Tests.Repositories;

[Collection(PostgresCollection.Name)]
public class MaintenanceRequestRepositoryTests
{
    private readonly PostgresFixture _fixture;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MaintenanceRequestRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Cam_Load_Request_Filter_By_Title()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql(_fixture.ConnectionString);
        var db = new AppDbContext(builder.Options);

        await db.Database.EnsureCreatedAsync();
        
        var repository = new MaintenanceRequestRepository(db);

        var result = await repository.GetPagedAsync("value",
            null,
            null,
            1,
            20,
            CompanyGuids.Acme, null,
            CancellationToken.None);
        
        Assert.NotNull(result);
    }
}