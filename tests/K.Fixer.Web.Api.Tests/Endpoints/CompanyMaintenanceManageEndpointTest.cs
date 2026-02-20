using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using K.Fixer.Application.Common;
using K.Fixer.Application.Maintenance.GetCompanyMaintenanceRequest;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Api.Features.Auth;
using K.Fixer.Web.Api.Features.CompanyMaintenanceManage.Models;
using K.Fixer.Web.Api.Tests.Extensions;
using K.Fixer.Web.Api.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Endpoints;


[Collection(PostgresCollection.Name)]
public class CompanyMaintenanceManageEndpointTest
{
    private const string CompanyAdminEmail = "victoria@acme.com";
    private const string TechnicianEmail = "anna@acme.com";
    private const string ResidentEmail = "daniel@acme.com";
    private const string Password = "fixer777";
    private const string AcmeGid = "eb11e34f-e00c-4631-b003-ca58ba7a366b";

    private readonly ITestOutputHelper _output;
    private readonly KFixerWebApplicationFactory _factory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompanyMaintenanceManageEndpointTest(PostgresFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _factory = new KFixerWebApplicationFactory(fixture.ConnectionString);
    }

    // ─── GET /api/company/requests ────────────────────────────────────────────

    [Fact]
    public async Task GetRequests_AsCompanyAdmin_Returns_Ok()
    {
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.GetAsync("/api/company/requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_ReturnsOnlyCompanyRequests()
    {
        var acmeBuildingId = await GetAcmeBuildingId();
        var otherBuildingId = await GetOtherCompanyBuildingId();
        var ownGid = await SeedRequest(acmeBuildingId);
        var otherGid = await SeedRequest(otherBuildingId);

        var client = await LoginAs(CompanyAdminEmail);
        var response = await client.GetAsync("/api/company/requests");
        var result = await response.ReadWithJson<PagedResult<MaintenanceRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result.Items, r => r.Id == ownGid);
        Assert.DoesNotContain(result.Items, r => r.Id == otherGid);
    }

    [Fact]
    public async Task GetRequests_FilterByStatus_ReturnsMatchingOnly()
    {
        var buildingId = await GetAcmeBuildingId();
        var newGid = await SeedRequest(buildingId, status: MaintenanceStatus.New.Value);
        var assignedGid = await SeedRequest(buildingId, status: MaintenanceStatus.Assigned.Value);

        var client = await LoginAs(CompanyAdminEmail);
        var response = await client.GetAsync("/api/company/requests?status=New");
        var result = await response.ReadWithJson<PagedResult<MaintenanceRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.All(result.Items, r => Assert.Equal(MaintenanceStatus.New.Value, r.Status));
        Assert.Contains(result.Items, r => r.Id == newGid);
        Assert.DoesNotContain(result.Items, r => r.Id == assignedGid);
    }

    [Fact]
    public async Task GetRequests_FilterByPriority_ReturnsMatchingOnly()
    {
        var buildingId = await GetAcmeBuildingId();
        var urgentGid = await SeedRequest(buildingId, priority: Priority.Urgent.Value);
        var lowGid = await SeedRequest(buildingId, priority: Priority.Low.Value);

        var client = await LoginAs(CompanyAdminEmail);
        var response = await client.GetAsync("/api/company/requests?priority=Urgent");
        var result = await response.ReadWithJson<PagedResult<MaintenanceRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.All(result.Items, r => Assert.Equal(Priority.Urgent.Value, r.Priority));
        Assert.Contains(result.Items, r => r.Id == urgentGid);
        Assert.DoesNotContain(result.Items, r => r.Id == lowGid);
    }

    [Fact]
    public async Task GetRequests_FilterByQuerySearch_ReturnsMatchingOnly()
    {
        var buildingId = await GetAcmeBuildingId();
        var matchGid = await SeedRequest(buildingId, title: "Broken valve in corridor");
        var noMatchGid = await SeedRequest(buildingId, title: "Elevator maintenance");

        var client = await LoginAs(CompanyAdminEmail);
        var response = await client.GetAsync("/api/company/requests?querySearch=valve");
        //var response = await client.GetAsync("/api/company/requests");
        var stringResponse = await response.Content.ReadAsStringAsync();
        var result = await response.ReadWithJson<PagedResult<MaintenanceRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result.Items, r => r.Id == matchGid);
        Assert.DoesNotContain(result.Items, r => r.Id == noMatchGid);
    }

    [Fact]
    public async Task GetRequests_SupportsPagination()
    {
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.GetAsync("/api/company/requests?pageNumber=1&pageSize=3");
        var result = await response.ReadWithJson<PagedResult<MaintenanceRequestListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Items.Count() <= 3);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(3, result.PageSize);
    }

    [Fact]
    public async Task GetRequests_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/company/requests");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_AsTechnician_Returns_Forbidden()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.GetAsync("/api/company/requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_AsResident_Returns_Forbidden()
    {
        var client = await LoginAs(ResidentEmail);

        var response = await client.GetAsync("/api/company/requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_AsAdmin_Returns_Forbidden()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);

        var response = await client.GetAsync("/api/company/requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ─── PUT /api/company/requests/{gid}/assign ───────────────────────────────

    [Fact]
    public async Task AssignTechnician_NewRequest_Returns_Ok()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: MaintenanceStatus.New.Value);
        var technicianGid = await GetTechnicianGid();
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(technicianGid));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_SetsStatusAndTechnicianInDb()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: MaintenanceStatus.New.Value);
        var technicianGid = await GetTechnicianGid();
        var client = await LoginAs(CompanyAdminEmail);

        await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(technicianGid));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);
        var technician = await db.Users.AsNoTracking().FirstAsync(u => u.Id == technicianGid);

        Assert.Equal(MaintenanceStatus.Assigned, saved.Status);
        Assert.Equal(technician.Id, saved.AssignedToId);
        Assert.NotNull(saved.ModifiedAt);
    }

    [Fact]
    public async Task AssignTechnician_CreatesStatusChangeLog()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId);
        var technicianGid = await GetTechnicianGid();
        var client = await LoginAs(CompanyAdminEmail);

        await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(technicianGid));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var req = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);
        var log = await db.Set<StatusChangeLogEntry>().AsNoTracking()
            .FirstOrDefaultAsync(l => l.RequestId == req.Id && l.NewStatus == MaintenanceStatus.Assigned.Value);

        Assert.NotNull(log);
        Assert.Equal(MaintenanceStatus.New.Value, log.OldStatus);
    }

    [Theory]
    [InlineData("Assigned")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Closed")]
    [InlineData("Reopened")]
    [InlineData("Cancelled")]
    public async Task AssignTechnician_NonNewStatus_Returns_BadRequest(string status)
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: status);
        var technicianGid = await GetTechnicianGid();
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(technicianGid));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_RequestNotFound_Returns_NotFound()
    {
        var technicianGid = await GetTechnicianGid();
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{Guid.NewGuid()}/assign",
            new AssignTechnicianRequest(technicianGid));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_OtherCompanyRequest_Returns_NotFound()
    {
        var otherBuildingId = await GetOtherCompanyBuildingId();
        var requestGid = await SeedRequest(otherBuildingId, status: MaintenanceStatus.New.Value);
        var technicianGid = await GetTechnicianGid();
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(technicianGid));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_TechnicianNotFound_Returns_NotFound()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: MaintenanceStatus.New.Value);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_UserIsNotTechnician_Returns_BadRequest()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: MaintenanceStatus.New.Value);
        var residentGid = await GetUserGid(ResidentEmail);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(residentGid));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_TechnicianFromOtherCompany_Returns_BadRequest()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: MaintenanceStatus.New.Value);
        var otherTechGid = await SeedTechnicianInOtherCompany();
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{requestGid}/assign",
            new AssignTechnicianRequest(otherTechGid));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _factory.CreateClient().PutAsJsonAsync(
            $"/api/company/requests/{Guid.NewGuid()}/assign",
            new AssignTechnicianRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AssignTechnician_AsTechnician_Returns_Forbidden()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PutAsJsonAsync(
            $"/api/company/requests/{Guid.NewGuid()}/assign",
            new AssignTechnicianRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ─── PATCH /api/company/requests/{gid}/priority ───────────────────────────

    [Theory]
    [InlineData("New")]
    [InlineData("Assigned")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Reopened")]
    public async Task ChangePriority_ActiveRequest_Returns_Ok(string status)
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: status, priority: Priority.Low.Value);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PatchAsJsonAsync(
            $"/api/company/requests/{requestGid}/priority",
            new ChangePriorityRequest(Priority.Urgent.Value));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangePriority_SavesNewPriorityInDb()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, priority: Priority.Low.Value);
        var client = await LoginAs(CompanyAdminEmail);

        await client.PatchAsJsonAsync(
            $"/api/company/requests/{requestGid}/priority",
            new ChangePriorityRequest(Priority.High.Value));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);

        Assert.Equal(Priority.High, saved.Priority);
        Assert.NotNull(saved.ModifiedAt);
    }

    [Theory]
    [InlineData("Closed")]
    [InlineData("Cancelled")]
    public async Task ChangePriority_ClosedOrCancelled_Returns_BadRequest(string status)
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: status);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PatchAsJsonAsync(
            $"/api/company/requests/{requestGid}/priority",
            new ChangePriorityRequest(Priority.Urgent.Value));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePriority_RequestNotFound_Returns_NotFound()
    {
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PatchAsJsonAsync(
            $"/api/company/requests/{Guid.NewGuid()}/priority",
            new ChangePriorityRequest(Priority.High.Value));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangePriority_OtherCompanyRequest_Returns_NotFound()
    {
        var otherBuildingId = await GetOtherCompanyBuildingId();
        var requestGid = await SeedRequest(otherBuildingId);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PatchAsJsonAsync(
            $"/api/company/requests/{requestGid}/priority",
            new ChangePriorityRequest(Priority.High.Value));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangePriority_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _factory.CreateClient().PatchAsJsonAsync(
            $"/api/company/requests/{Guid.NewGuid()}/priority",
            new ChangePriorityRequest(Priority.High.Value));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePriority_AsTechnician_Returns_Forbidden()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PatchAsJsonAsync(
            $"/api/company/requests/{Guid.NewGuid()}/priority",
            new ChangePriorityRequest(Priority.High.Value));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ─── POST /api/company/requests/{gid}/cancel ──────────────────────────────

    [Theory]
    [InlineData("New")]
    [InlineData("Assigned")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Reopened")]
    public async Task CancelRequest_ActiveRequest_Returns_Ok(string status)
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: status);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PostAsync($"/api/company/requests/{requestGid}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelRequest_SetsStatusCancelledInDb()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: MaintenanceStatus.New.Value);
        var client = await LoginAs(CompanyAdminEmail);

        await client.PostAsync($"/api/company/requests/{requestGid}/cancel", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);

        Assert.Equal(MaintenanceStatus.Cancelled, saved.Status);
        Assert.NotNull(saved.ModifiedAt);
    }

    [Fact]
    public async Task CancelRequest_CreatesStatusChangeLog()
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: MaintenanceStatus.InProgress.Value);
        var client = await LoginAs(CompanyAdminEmail);

        await client.PostAsync($"/api/company/requests/{requestGid}/cancel", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var req = await db.Requests.AsNoTracking().FirstAsync(r => r.Id == requestGid);
        var log = await db.Set<StatusChangeLogEntry>().AsNoTracking()
            .FirstOrDefaultAsync(l => l.RequestId == req.Id && l.NewStatus == MaintenanceStatus.Cancelled.Value);

        Assert.NotNull(log);
        Assert.Equal(MaintenanceStatus.InProgress.Value, log.OldStatus);
    }

    [Theory]
    [InlineData("Closed")]
    [InlineData("Cancelled")]
    public async Task CancelRequest_AlreadyClosedOrCancelled_Returns_BadRequest(string status)
    {
        var buildingId = await GetAcmeBuildingId();
        var requestGid = await SeedRequest(buildingId, status: status);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PostAsync($"/api/company/requests/{requestGid}/cancel", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelRequest_RequestNotFound_Returns_NotFound()
    {
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PostAsync($"/api/company/requests/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelRequest_OtherCompanyRequest_Returns_NotFound()
    {
        var otherBuildingId = await GetOtherCompanyBuildingId();
        var requestGid = await SeedRequest(otherBuildingId);
        var client = await LoginAs(CompanyAdminEmail);

        var response = await client.PostAsync($"/api/company/requests/{requestGid}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelRequest_Unauthenticated_Returns_Unauthorized()
    {
        var response = await _factory.CreateClient()
            .PostAsync($"/api/company/requests/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CancelRequest_AsTechnician_Returns_Forbidden()
    {
        var client = await LoginAs(TechnicianEmail);

        var response = await client.PostAsync($"/api/company/requests/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelRequest_AsResident_Returns_Forbidden()
    {
        var client = await LoginAs(ResidentEmail);

        var response = await client.PostAsync($"/api/company/requests/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

    private async Task<Guid> GetAcmeBuildingId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var acme = await db.Companies.AsNoTracking().FirstAsync(c => c.Id == Guid.Parse(AcmeGid));
        return await db.BuildingsReads
            .Where(b => b.CompanyId == acme.Id)
            .Select(b => b.Id)
            .FirstAsync();
    }

    private async Task<Guid> GetOtherCompanyBuildingId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var acme = await db.Companies.AsNoTracking().FirstAsync(c => c.Id == Guid.Parse(AcmeGid));
        return await db.BuildingsReads
            .Where(b => b.CompanyId != acme.Id)
            .Select(b => b.Id)
            .FirstAsync();
    }

    private async Task<Guid> GetTechnicianGid()
        => await GetUserGid(TechnicianEmail);

    private async Task<Guid> GetUserGid(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Users.AsNoTracking()
            .Where(u => u.Email == Email.Create(email).Value)
            .Select(u => u.Id)
            .SingleAsync();
    }

    private async Task<Guid> SeedTechnicianInOtherCompany()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var acme = await db.Companies.AsNoTracking().FirstAsync(c => c.Id == Guid.Parse(AcmeGid));
        var otherCompany = await db.Companies.AsNoTracking()
            .FirstAsync(c => c.Id != acme.Id);
        var tech = User.Create($"tech-{Guid.NewGuid():N}@other.com", "fixer777", "Other Company Technician",
            UserRole.Technician.Value, otherCompany.Id);
        
        db.Users.Add(tech.Value!);
        await db.SaveChangesAsync();
        return tech.Value!.Id;
    }

    private async Task<Guid> SeedRequest(
        Guid buildingId,
        string title = "Test Request",
        string description = "Test Description",
        string status = "New",
        string priority = "Medium")
    {
        var userId = await GetUserGid("daniel@acme.com");
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = MaintenanceRequest.Create( buildingId, userId, title, description);
       
      
        
        db.Requests.Add(request.Value!);
        await db.SaveChangesAsync();
        var requestId = request.Value!.Id;
        if (status != MaintenanceStatus.New.Value)
        {
            await db.Database.ExecuteSqlAsync(
                $"update maintenance_requests set status = {status} where gid = {requestId}");
        }
        
        if (priority != Priority.Low.Value)
        {
            await db.Database.ExecuteSqlAsync(
                $"update maintenance_requests set priority = {priority} where gid = {requestId}");
        }

        return requestId;
    }
}
