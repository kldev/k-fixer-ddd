using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using K.Fixer.Application.Common;
using K.Fixer.Application.Maintenance.GetTechnicianRequests;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Api.Features.Auth;
using K.Fixer.Web.Api.Features.TechnicianTasks.Model;
using K.Fixer.Web.Api.Tests.Extensions;
using K.Fixer.Web.Api.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Endpoints;


[Collection(PostgresCollection.Name)]
public class TechnicianTasksEndpointTest
{
    private const string TechnicianEmail = "anna@acme.com";
    private const string ResidentEmail = "daniel@acme.com";
    private const string Password = "fixer777";
    private const string AcmeGid = "eb11e34f-e00c-4631-b003-ca58ba7a366b";

    private readonly ITestOutputHelper _output;
    private readonly KFixerWebApplicationFactory _factory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TechnicianTasksEndpointTest(PostgresFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _factory = new KFixerWebApplicationFactory(fixture.ConnectionString);
    }

    // ─── GET /api/technician/requests ─────────────────────────────────────────

    [Fact]
    public async Task GetRequests_AsTechnician_Returns_Ok()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.GetAsync("/api/technician/requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_ReturnsOnlyOwnRequests()
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var ownGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.Assigned.Value);

        var otherTechId = await SeedTechnicianUser();
        var otherGid = await SeedRequest(otherTechId, buildingId, status: MaintenanceStatus.Assigned.Value);

        var client = await LoginAs(TechnicianEmail);
        var response = await client.GetAsync("/api/technician/requests");
        var result = await response.ReadWithJson<PagedResult<TechnicianRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result.Items, r => r.Gid == ownGid);
        Assert.DoesNotContain(result.Items, r => r.Gid == otherGid);
    }

    [Fact]
    public async Task GetRequests_FilterByStatus_ReturnsMatchingOnly()
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var assignedGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.Assigned.Value);
        var inProgressGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.InProgress.Value);

        var client = await LoginAs(TechnicianEmail);
        var response = await client.GetAsync("/api/technician/requests?status=Assigned");
        var result = await response.ReadWithJson<PagedResult<TechnicianRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.All(result.Items, r => Assert.Equal(MaintenanceStatus.Assigned.Value, r.Status));
        Assert.Contains(result.Items, r => r.Gid == assignedGid);
        Assert.DoesNotContain(result.Items, r => r.Gid == inProgressGid);
    }

    [Fact]
    public async Task GetRequests_SupportsPagination()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.GetAsync("/api/technician/requests?pageNumber=1&pageSize=5");
        var result = await response.ReadWithJson<PagedResult<TechnicianRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Items.Count() <= 5);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
    }

    [Fact]
    public async Task GetRequests_Unauthenticated_Returns_Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/technician/requests");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_AsResident_Returns_UnprocessableEntity()
    {
        var client = await LoginAs(ResidentEmail);

        var response = await client.GetAsync("/api/technician/requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_AsCompanyAdmin_Returns_UnprocessableEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var response = await client.GetAsync("/api/technician/requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ─── POST /api/technician/requests/{gid}/start ────────────────────────────

    [Theory]
    [InlineData("Assigned")]
    [InlineData("Reopened")]
    public async Task StartRequest_FromValidStatus_Returns_Ok(string initialStatus)
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: initialStatus);
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsync($"/api/technician/requests/{requestGid}/start", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("Assigned")]
    [InlineData("Reopened")]
    public async Task StartRequest_SetsStatusInProgressAndStartDate(string initialStatus)
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: initialStatus);
        var client = await LoginAs(TechnicianEmail);

        await client.PostAsync($"/api/technician/requests/{requestGid}/start", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);

        Assert.Equal(MaintenanceStatus.InProgress, saved.Status);
        Assert.NotNull(saved.WorkPeriod);
        Assert.NotNull(saved.ModifiedAt);
        
    }

    [Theory]
    [InlineData("Assigned")]
    [InlineData("Reopened")]
    public async Task StartRequest_CreatesStatusChangeLog(string initialStatus)
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: initialStatus);
        var client = await LoginAs(TechnicianEmail);

        await client.PostAsync($"/api/technician/requests/{requestGid}/start", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var req = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);
        var log = await db.StatusChanges.AsNoTracking()
            .FirstOrDefaultAsync(l => l.RequestId == req.Id && l.NewStatus == MaintenanceStatus.InProgress.Value);

        Assert.NotNull(log);
        Assert.Equal(initialStatus, log.OldStatus);
    }

    [Theory]
    [InlineData("New")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Closed")]
    [InlineData("Cancelled")]
    public async Task StartRequest_FromInvalidStatus_Returns_BadRequest(string invalidStatus)
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: invalidStatus);
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsync($"/api/technician/requests/{requestGid}/start", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartRequest_NotFound_Returns_NotFound()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsync($"/api/technician/requests/{Guid.NewGuid()}/start", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StartRequest_AnotherTechnicianRequest_Returns_BadRequest()
    {
        var (_, buildingId) = await GetTechnicianInfo();
        var otherTechId = await SeedTechnicianUser();
        var requestGid = await SeedRequest(otherTechId, buildingId, status: MaintenanceStatus.Assigned.Value);

        var client = await LoginAs(TechnicianEmail);
        var response = await client.PostAsync($"/api/technician/requests/{requestGid}/start", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartRequest_Unauthenticated_Returns_Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/technician/requests/{Guid.NewGuid()}/start", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ─── POST /api/technician/requests/{gid}/complete ─────────────────────────

    [Fact]
    public async Task CompleteRequest_FromInProgress_Returns_Ok()
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.InProgress.Value);
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsJsonAsync(
            $"/api/technician/requests/{requestGid}/complete",
            new CompleteRequestBody("Naprawiono zawór"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompleteRequest_SetsStatusAndFields()
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.InProgress.Value);
        var client = await LoginAs(TechnicianEmail);

        await client.PostAsJsonAsync(
            $"/api/technician/requests/{requestGid}/complete",
            new CompleteRequestBody("Naprawiono zawór"));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);

        Assert.Equal(MaintenanceStatus.Completed, saved.Status);
        Assert.Equal("Naprawiono zawór", saved.ResolutionNote.Value);
        Assert.NotNull(saved.WorkPeriod!.CompletedAt);
        Assert.NotNull(saved.ModifiedAt);
        
    }

    [Fact]
    public async Task CompleteRequest_CreatesStatusChangeLog()
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.InProgress.Value);
        var client = await LoginAs(TechnicianEmail);

        await client.PostAsJsonAsync(
            $"/api/technician/requests/{requestGid}/complete",
            new CompleteRequestBody("Praca wykonana"));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var req = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);
        var log = await db.StatusChanges.AsNoTracking()
            .FirstOrDefaultAsync(l => l.RequestId == req.Id && l.NewStatus == MaintenanceStatus.Completed.Value);

        Assert.NotNull(log);
        Assert.Equal(MaintenanceStatus.InProgress.Value, log.OldStatus);
    }

    [Theory]
    [InlineData("Assigned")]
    [InlineData("Reopened")]
    [InlineData("Completed")]
    [InlineData("Closed")]
    [InlineData("Cancelled")]
    [InlineData("New")]
    public async Task CompleteRequest_FromInvalidStatus_Returns_BadRequest(string invalidStatus)
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: invalidStatus);
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsJsonAsync(
            $"/api/technician/requests/{requestGid}/complete",
            new CompleteRequestBody("Notatka"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CompleteRequest_EmptyResolutionNote_Returns_BadRequest(string note)
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.InProgress.Value);
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsJsonAsync(
            $"/api/technician/requests/{requestGid}/complete",
            new CompleteRequestBody(note));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteRequest_ResolutionNoteTooLong_Returns_BadRequest()
    {
        var (techId, buildingId) = await GetTechnicianInfo();
        var requestGid = await SeedRequest(techId, buildingId, status: MaintenanceStatus.InProgress.Value);
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsJsonAsync(
            $"/api/technician/requests/{requestGid}/complete",
            new CompleteRequestBody(new string('x', 2001)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteRequest_NotFound_Returns_NotFound()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsJsonAsync(
            $"/api/technician/requests/{Guid.NewGuid()}/complete",
            new CompleteRequestBody("Notatka"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompleteRequest_AnotherTechnicianRequest_Returns_BadRequest()
    {
        var (_, buildingId) = await GetTechnicianInfo();
        var otherTechId = await SeedTechnicianUser();
        var requestGid = await SeedRequest(otherTechId, buildingId, status: MaintenanceStatus.InProgress.Value);

        var client = await LoginAs(TechnicianEmail);
        var response = await client.PostAsJsonAsync(
            $"/api/technician/requests/{requestGid}/complete",
            new CompleteRequestBody("Notatka"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteRequest_Unauthenticated_Returns_Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/technician/requests/{Guid.NewGuid()}/complete",
            new CompleteRequestBody("Notatka"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<HttpClient> LoginAs(string email)
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));
        var session = await loginResponse.ReadWithJson<SessionResponse>(_output);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.Token);
        return client;
    }

    private async Task<(Guid TechnicianId, Guid BuildingId)> GetTechnicianInfo()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tech = await db.Users.AsNoTracking()
            .FirstAsync(u => u.Email == Email.Create(TechnicianEmail).Value);
        var building = await db.BuildingsReads
            .FirstAsync();
        return (tech.Id, building.Id);
    }

    private async Task<Guid> SeedTechnicianUser()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var acme = await db.Companies.FirstAsync(c => c.Id == Guid.Parse(AcmeGid));
        var random = Guid.NewGuid().ToString("N").Substring(0,8);
        var tech = User.Create($"tech-{random}@cme.com", Password, "Extra Technician", UserRole.Technician.Value,
            acme.Id);
        
        db.Users.Add(tech.Value!);
        await db.SaveChangesAsync();
        return tech.Value!.Id;
    }
    
    private async Task<Guid> SeedRequest(
        Guid assignedToId,
        Guid buildingId,
        string title = "Test Request",
        string description = "Test Description",
        string status = "Assigned")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = MaintenanceRequest.Create(buildingId, Guid.NewGuid(), title, description);

        if (status != MaintenanceStatus.New.Value)
        {
            request.Value!.AssignTo(assignedToId, assignedToId);
        }

        if (status == MaintenanceStatus.InProgress.Value)
        {
            request.Value!.StartWork(assignedToId);
        }

        db.Requests.Add(request.Value!);
        await db.SaveChangesAsync();

        var requestId = request.Value!.Id;

        if (status != MaintenanceStatus.New.Value &&
            status != MaintenanceStatus.Assigned.Value &&
            status != MaintenanceStatus.InProgress.Value)
        {
            await db.Database.ExecuteSqlAsync(
                $"update maintenance_requests set status = {status} where gid = {requestId}");
        }

        return requestId;
    }


}
