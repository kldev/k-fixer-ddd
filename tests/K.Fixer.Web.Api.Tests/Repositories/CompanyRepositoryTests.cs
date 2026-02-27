using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Infrastructure.PropertyManagement.Repositories;
using K.Fixer.Web.Api.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Repositories;

[Collection(PostgresCollection.Name)]
public class CompanyRepositoryTests
{
    private readonly PostgresFixture _fixture;
    private readonly ITestOutputHelper _output;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompanyRepositoryTests(PostgresFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Can_Get_Companies_Filter_By_QuerySearch()
    {
        AppDbContext db = await GetAppDbContext();

        var repo = new CompanyRepository(db);
        var result = await repo.GetPagedAsync("Test", 1, 20, CancellationToken.None);

        Assert.NotNull(result);
    }

    private async Task<AppDbContext> GetAppDbContext()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql(_fixture.ConnectionString);
        builder.LogTo(_output.WriteLine, LogLevel.Information);
        var db = new AppDbContext(builder.Options);

        await db.Database.EnsureCreatedAsync();
        return db;
    }
}