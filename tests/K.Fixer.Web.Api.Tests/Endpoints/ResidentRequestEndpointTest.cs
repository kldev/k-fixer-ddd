using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Api.Features.Auth;
using K.Fixer.Web.Api.Features.ResidentRequests.Model;
using K.Fixer.Web.Api.Tests.Extensions;
using K.Fixer.Web.Api.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Endpoints;

[Collection(PostgresCollection.Name)]
public class ResidentRequestEndpointTest
{
    private const string DanielEmail = "daniel@acme.com";
    private const string AnotherResidentEmail = "m.green@acme.com"; // seeded by ResidentBuildingSeeder
    private const string TechnicianEmail = "anna@acme.com";
    private const string Password = "fixer777";

    private readonly ITestOutputHelper _output;
    private readonly KFixerWebApplicationFactory _factory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ResidentRequestEndpointTest(PostgresFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _factory = new KFixerWebApplicationFactory(fixture.ConnectionString);
    }

    // --- Happy path ---
    
    [Fact]
    public async Task Resident_Create_OwnNewRequest_Returns_Ok()
    {
        var client = await LoginAs(DanielEmail);

        var response = await client.PostAsJsonAsync(
            $"/resident-requests",
            new ResidentMaintenanceRequest("Test Title", "Test Description"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Resident_Create_OwnNewRequest_Returns_BadRequest()
    {
        var client = await LoginAs(DanielEmail);

        var response = await client.PostAsJsonAsync(
            $"/resident-requests",
            new ResidentMaintenanceRequest("Test Title", new string('a', 2001)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Resident_Update_OwnNewRequest_Returns_Ok()
    {
        var (userId, buildingId) = await GetUserInfo(DanielEmail);
        var requestGid = await SeedRequest(userId, buildingId);
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest("Updated Title", "Updated Description"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Resident_Update_ChangesAreSaved()
    {
        var (userId, buildingId) = await GetUserInfo(DanielEmail);
        var requestGid = await SeedRequest(userId, buildingId);
        var client = await LoginAs(DanielEmail);

        await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest("New Title", "New Description"));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);

        Assert.Equal("New Title", saved.Title.Value);
        Assert.Equal("New Description", saved.Description.Value);
        Assert.NotNull(saved.ModifiedAt);
        }

    // --- Authorization ---

    [Fact]
    public async Task Unauthenticated_Update_Returns_Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{Guid.NewGuid()}",
            new ResidentMaintenanceRequest("Title", "Description"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Technician_Update_Returns_NotFound_For_Non_Existing_Request()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{Guid.NewGuid()}",
            new ResidentMaintenanceRequest("Title", "Description"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Business logic ---

    [Fact]
    public async Task Resident_Update_RequestNotFound_ReturnsBadRequest()
    {
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{Guid.NewGuid()}",
            new ResidentMaintenanceRequest("Title", "Description"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Resident_Update_NotOwner_Returns_BadRequest()
    {
        var (otherUserId, otherBuildingId) = await GetUserInfo(AnotherResidentEmail);
        var requestGid = await SeedRequest(otherUserId, otherBuildingId);
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest("Title", "Description"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("Assigned")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Closed")]
    [InlineData("Reopened")]
    [InlineData("Cancelled")]
    public async Task Resident_Update_NonNewStatus_Returns_BadRequest(string status)
    {
        var (userId, buildingId) = await GetUserInfo(DanielEmail);
        var requestGid = await SeedRequest(userId, buildingId, status: status);
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest("Title", "Description"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Validation ---

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Resident_Update_EmptyTitle_Returns_BadRequest(string title)
    {
        var (userId, buildingId) = await GetUserInfo(DanielEmail);
        var requestGid = await SeedRequest(userId, buildingId);
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest(title, "Valid description"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Resident_Update_TitleTooLong_Returns_BadRequest()
    {
        var (userId, buildingId) = await GetUserInfo(DanielEmail);
        var requestGid = await SeedRequest(userId, buildingId);
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest(new string('a', 201), "Valid description"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Resident_Update_EmptyDescription_Returns_BadRequest(string description)
    {
        var (userId, buildingId) = await GetUserInfo(DanielEmail);
        var requestGid = await SeedRequest(userId, buildingId);
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest("Valid title", description));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Resident_Update_DescriptionTooLong_Returns_BadRequest()
    {
        var (userId, buildingId) = await GetUserInfo(DanielEmail);
        var requestGid = await SeedRequest(userId, buildingId);
        var client = await LoginAs(DanielEmail);

        var response = await client.PutAsJsonAsync(
            $"/resident-requests/{requestGid}",
            new ResidentMaintenanceRequest("Valid title", new string('a', 2001)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Helpers ---

    private async Task<HttpClient> LoginAs(string email)
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));
        var session = await loginResponse.ReadWithJson<SessionResponse>(_output);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.Token);
        return client;
    }

    private async Task<(Guid UserId, Guid BuildingId)> GetUserInfo(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.UsersReads.Where(z => z.Email == Email.Create(email).Value).FirstOrDefaultAsync();
        var residentOccupancy = await db.Set<ResidentOccupancy>()
            .Where(z=>z.ResidentId == user.Id && z.Period.To == null)
            .FirstOrDefaultAsync();
        
        return (user!.Id, residentOccupancy!.BuildingId);
    }

    private async Task<Guid> SeedRequest(
        Guid userId,
        Guid buildingId,
        string title = "Original Title",
        string description = "Original Description",
        string status = "New")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = MaintenanceRequest.Create( buildingId, userId, title, description);
        
        db.Requests.Add(request.Value!);
        await db.SaveChangesAsync();
        var requestId = db.RequestsReads.FirstOrDefault(z => z.Title == RequestTitle.Create(title).Value)?.Id;

        if (status != MaintenanceStatus.New.Value)
        {
            await db.Database.ExecuteSqlAsync(
                $"update maintenance_requests set status = {status} where gid = {requestId.Value}");
        }
        
        return requestId!.Value;
    }

}