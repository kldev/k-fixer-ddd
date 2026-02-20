using System.Net;
using System.Net.Http.Json;

using K.Fixer.Application.Common;
using K.Fixer.Application.PropertyManagement.GetCompanyBuildings;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Web.Api.Features.CompanyManage.Model;
using K.Fixer.Web.Api.Tests.Extensions;
using K.Fixer.Web.Api.Tests.Infrastructure;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Endpoints;


/// <summary>
/// Tests for CompanyAdmin self-manage endpoint: /api/company/building
/// Company is resolved from JWT claims, no companyGid in URL.
/// </summary>
[Collection(PostgresCollection.Name)]
public class CompanyBuildingsSelfEndpointTest
{
    private readonly ITestOutputHelper _output;
    private readonly KFixerWebApplicationFactory _factory;

    private const string AcmeGid = "eb11e34f-e00c-4631-b003-ca58ba7a366b";

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompanyBuildingsSelfEndpointTest(PostgresFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _factory = new KFixerWebApplicationFactory(fixture.ConnectionString);
    }

    // --- CompanyAdmin: GetBuildings ---

    [Fact]
    public async Task CompanyAdmin_GetBuildings_Returns_Ok_With_PagedResult()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var response = await client.GetAsync("/api/company/building");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);
        Assert.NotNull(paged);
        Assert.NotEmpty(paged.Items);
        Assert.True(paged.TotalCount > 0);
    }

    [Fact]
    public async Task CompanyAdmin_GetBuildings_QuerySearch_Filters_ByName()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var response = await client.GetAsync("/api/company/building?querySearch=Main+Office");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);
        Assert.NotNull(paged);
        Assert.Single(paged.Items);
        Assert.Equal("Main Office", paged.Items.First().Name);
    }

    [Fact]
    public async Task CompanyAdmin_GetBuildings_QuerySearch_NoMatch_Returns_Empty()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var response = await client.GetAsync("/api/company/building?querySearch=ZZNONEXISTENT999");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);
        Assert.NotNull(paged);
        Assert.Empty(paged.Items);
        Assert.Equal(0, paged.TotalCount);
    }

    [Fact]
    public async Task CompanyAdmin_GetBuildings_Pagination_ReturnsCorrectPage()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var response1 = await client.GetAsync("/api/company/building?pageSize=2&pageNumber=1");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var page1 = await response1.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);
        Assert.NotNull(page1);
        Assert.Equal(2, page1.Items.Count());
        Assert.True(page1.HasNextPage);
        Assert.False(page1.HasPreviousPage);

        var response2 = await client.GetAsync("/api/company/building?pageSize=2&pageNumber=2");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var page2 = await response2.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);
        Assert.NotNull(page2);
        Assert.NotEmpty(page2.Items);
        Assert.True(page2.HasPreviousPage);
    }

    // --- CompanyAdmin: CreateBuilding ---

    [Fact]
    public async Task CompanyAdmin_CreateBuilding_Returns_Ok()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var request = new CreateCompanyBuildingRequest("CA Self Building " + uniqueId, "1 CA Street " + uniqueId, "City", "IT-222", "BE" );
        var response = await client.PostAsJsonAsync("/api/company/building", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompanyAdmin_CreateBuilding_EmptyName_Returns_BadRequest()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var request = new CreateCompanyBuildingRequest("", "1 Test Street", "City", "IT-222", "BE" );
        var response = await client.PostAsJsonAsync("/api/company/building", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompanyAdmin_CreateBuilding_EmptyAddress_Returns_BadRequest()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var request = new CreateCompanyBuildingRequest("Some Building", "", "City", "IT-222", "BE" );
        var response = await client.PostAsJsonAsync("/api/company/building", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompanyAdmin_CreateBuilding_DuplicateName_Returns_UnprocessableEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var name = "CA Dup " + uniqueId;

        await client.PostAsJsonAsync("/api/company/building", new CreateCompanyBuildingRequest(name, "Addr 1", "City", "IT-222", "BE" ));
        var response = await client.PostAsJsonAsync("/api/company/building", new CreateCompanyBuildingRequest(name, "Addr 2", "City", "IT-222", "BE" ));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // --- CompanyAdmin: UpdateBuilding ---

    [Fact]
    public async Task CompanyAdmin_UpdateBuilding_Returns_Ok()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var name = "CA Update Target " + uniqueId;

        await client.PostAsJsonAsync("/api/company/building", new CreateCompanyBuildingRequest(name, "Initial Address", "City", "IT-222", "BE" ));

        var listResponse = await client.GetAsync($"/api/company/building?querySearch={uniqueId}");
        var paged = await listResponse.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);
        var buildingGid = paged!.Items.First().Gid;

        var response = await client.PutAsJsonAsync($"/api/company/building/{buildingGid}",
          new UpdateCompanyBuildingRequest("CA Updated " + uniqueId, "Updated Address", "City", "IT-222", "BE" ));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompanyAdmin_UpdateBuilding_NotFound_Returns_NotFound()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var response = await client.PutAsJsonAsync($"/api/company/building/{Guid.NewGuid()}",
          new UpdateCompanyBuildingRequest("Updated Name", "Updated Address", "City", "IT-222", "BE" ));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompanyAdmin_UpdateBuilding_DuplicateName_Returns_UnprocessableEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.CompanyAdmin, AcmeGid);

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var nameA = "CA ConflictA " + uniqueId;
        var nameB = "CA ConflictB " + uniqueId;

        await client.PostAsJsonAsync("/api/company/building", new CreateCompanyBuildingRequest(nameA, "Addr A", "City", "IT-222", "BE" ));
        await client.PostAsJsonAsync("/api/company/building", new CreateCompanyBuildingRequest(nameB, "Addr B", "City", "IT-222", "BE" ));

        var listResponse = await client.GetAsync($"/api/company/building?querySearch={uniqueId}");
        var paged = await listResponse.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);
        var bGid = paged!.Items.First(b => b.Name == nameB).Gid;

        var response = await client.PutAsJsonAsync($"/api/company/building/{bGid}",
          new UpdateCompanyBuildingRequest(nameA, "Some Address", "City", "IT-222", "BE" ));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // --- Non-CompanyAdmin roles: forbidden ---

    [Fact]
    public async Task Admin_GetBuildings_Via_Self_Returns_ForbiddenEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);

        var response = await client.GetAsync("/api/company/building");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CreateBuilding_Via_Self_Returns_ForbiddenEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);

        var request = new CreateCompanyBuildingRequest("Admin Self Building", "Some Address", "City", "IT-222", "BE" );
        var response = await client.PostAsJsonAsync("/api/company/building", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Technician_CreateBuilding_Returns_ForbiddenEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Technician, AcmeGid);

        var request = new CreateCompanyBuildingRequest("Tech Building", "Tech Address", "City", "IT-222", "BE" );
        var response = await client.PostAsJsonAsync("/api/company/building", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Resident_GetBuildings_Returns_ForbiddenEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Resident, AcmeGid);

        var response = await client.GetAsync("/api/company/building");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- Unauthenticated ---

    [Fact]
    public async Task Unauthenticated_GetBuildings_Returns_Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/company/building");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_CreateBuilding_Returns_Unauthorized()
    {
        var client = _factory.CreateClient();

        var request = new CreateCompanyBuildingRequest("No Auth Building", "No Auth Address", "City", "IT-222", "BE" );
        var response = await client.PostAsJsonAsync("/api/company/building", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}