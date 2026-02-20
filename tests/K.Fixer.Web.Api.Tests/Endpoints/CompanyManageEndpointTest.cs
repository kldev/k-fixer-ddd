using System.Net;
using System.Net.Http.Json;

using K.Fixer.Application.Common;
using K.Fixer.Application.PropertyManagement.CreateCompanyBuilding;
using K.Fixer.Application.PropertyManagement.GetCompanies;
using K.Fixer.Application.PropertyManagement.GetCompanyBuildings;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Web.Api.Features.CompanyManage.Model;
using K.Fixer.Web.Api.Tests.Extensions;
using K.Fixer.Web.Api.Tests.Infrastructure;
using K.Fixer.Web.Seed.Helpers;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Endpoints;


[Collection(PostgresCollection.Name)]
public class CompanyManageEndpointTest
{
    private static Random random = new ();
    private readonly ITestOutputHelper _output;
    private readonly KFixerWebApplicationFactory _factory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompanyManageEndpointTest(PostgresFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _factory = new KFixerWebApplicationFactory(fixture.ConnectionString);
    }

    private async Task<Guid> CreateCompanyAndGetGidAsync(HttpClient client)
    {
        var nip  = NipHelper.GenerateValidNip(random);
        var name = "Test " + Guid.NewGuid().ToString("N")[..8];
        await client.PostAsJsonAsync("/api/company-manage", new CreateCompanyRequest(name, nip, "contact@fake.io", null));

        var listResponse = await client.GetAsync($"/api/company-manage?querySearch={Uri.EscapeDataString(name)}");
        var paged = await listResponse.ReadWithJson<PagedResult<CompanyListItemDto>>(_output);
        return paged!.Items.First(c => c.Name == name).Gid;
    }

    private static CreateCompanyBuildingRequest ValidBuildingRequest(string suffix = "") =>
        new("Blok " + suffix, "ul. Testowa 1", "Warszawa", "00-001", "PL");
    
    [Fact]
    public async Task Admin_Can_Create_Company()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);
        var name = "Test "  + Guid.NewGuid().ToString("N")[..8];
        var nip = NipHelper.GenerateValidNip(random);
            
        var request1 = new CreateCompanyRequest(name, nip,"contact@fake.io", null);
        var response1 = await client.PostAsJsonAsync("/api/company-manage", request1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
    }
    
    [Fact]
    public async Task Admin_Can_Create_Company_Should_Fail_For_Technician()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Technician);
        var name = "Test "  + Guid.NewGuid().ToString("N")[..8];
        var nip = NipHelper.GenerateValidNip(random);
            
        var request1 = new CreateCompanyRequest(name, nip,"contact@fake.io", null);
        var response1 = await client.PostAsJsonAsync("/api/company-manage", request1);
        Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);
    }
    
    
    [Fact]
    public async Task Admin_CreateCompany_DuplicateName_Returns_UnprocessableEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);
        var name = "Duplicate Name Test " + Guid.NewGuid().ToString("N")[..8];
        var nip = NipHelper.GenerateValidNip(random);
        var nip2 = NipHelper.GenerateValidNip(random);
        
        var request1 = new CreateCompanyRequest(name, nip,"company1@fake.io", null);
        var response1 = await client.PostAsJsonAsync("/api/company-manage", request1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var request2 = new CreateCompanyRequest(name, nip2, "company2@fake.io", null);
        var response2 = await client.PostAsJsonAsync("/api/company-manage", request2);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);
    }
    
    
    [Fact]
    public async Task Admin_CreateCompany_DuplicateTaxId_Returns_UnprocessableEntity()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);
        var taxId = NipHelper.GenerateValidNip(random);

        var request1 = new CreateCompanyRequest("Company TaxDup A " + taxId, taxId, "companyA@fake.mail", null);
        var response1 = await client.PostAsJsonAsync("/api/company-manage", request1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var request2 = new CreateCompanyRequest("Company TaxDup B " + taxId, taxId, "companyA@fake.mail", null);
        var response2 = await client.PostAsJsonAsync("/api/company-manage", request2);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);
    }
    
    [Fact]
    public async Task Admin_UpdateCompany_Returns_Ok()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);
        var taxId = NipHelper.GenerateValidNip(random);
        // create
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new CreateCompanyRequest("Update Test " + uniqueId, taxId,"UPD" + uniqueId+"@fake.io", null);
        await client.PostAsJsonAsync("/api/company-manage", createRequest);
    
        // get list to find Gid
        var listResponse = await client.GetAsync($"/api/company-manage?querySearch=Update+Test+{uniqueId}");
        var paged = await listResponse.ReadWithJson<PagedResult<CompanyListItemDto>>(_output);
        var company = paged!.Items.First(c => c.Name == "Update Test " + uniqueId);
    
        // update
        var updateRequest = new UpdateCompanyRequest("Updated Name " + uniqueId, "UPD" + uniqueId+"@fake.io", "new@email.com");
        var response = await client.PutAsJsonAsync($"/api/company-manage/{company.Gid}", updateRequest);
    
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_UpdateCompany_NotFound_Returns_NotFound()
    {
        var client = _factory.CreateClient().WithAuth(UserRole.Admin);
        var nip = NipHelper.GenerateValidNip(random);

        var updateRequest = new UpdateCompanyRequest("Some Name", "contact@someName.fake", null);
        var response = await client.PutAsJsonAsync($"/api/company-manage/{Guid.NewGuid().ToString()}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── CreateBuilding ────────────────────────────────────────────────────────

    [Fact]
    public async Task Admin_Can_Create_Building()
    {
        var client     = _factory.CreateClient().WithAuth(UserRole.Admin);
        var companyGid = await CreateCompanyAndGetGidAsync(client);

        var response = await client.PostAsJsonAsync(
            $"/api/company-manage/{companyGid}/building",
            ValidBuildingRequest("A"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CreateBuilding_Technician_Returns_Forbidden()
    {
        var adminClient = _factory.CreateClient().WithAuth(UserRole.Admin);
        var companyGid  = await CreateCompanyAndGetGidAsync(adminClient);

        var client   = _factory.CreateClient().WithAuth(UserRole.Technician);
        var response = await client.PostAsJsonAsync(
            $"/api/company-manage/{companyGid}/building",
            ValidBuildingRequest("B"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CreateBuilding_CompanyNotFound_Returns_NotFound()
    {
        var client   = _factory.CreateClient().WithAuth(UserRole.Admin);
        var response = await client.PostAsJsonAsync(
            $"/api/company-manage/{Guid.NewGuid()}/building",
            ValidBuildingRequest("C"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CreateBuilding_InvalidPostalCode_Returns_BadRequest()
    {
        var client     = _factory.CreateClient().WithAuth(UserRole.Admin);
        var companyGid = await CreateCompanyAndGetGidAsync(client);

        // "12345" is not a valid Polish postal code (expected format XX-XXX)
        var request  = new CreateCompanyBuildingRequest("Blok D", "ul. Testowa 1", "Warszawa", "12345", "PL");
        var response = await client.PostAsJsonAsync(
            $"/api/company-manage/{companyGid}/building", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── UpdateBuilding ────────────────────────────────────────────────────────

    [Fact]
    public async Task Admin_Can_Update_Building()
    {
        var client     = _factory.CreateClient().WithAuth(UserRole.Admin);
        var companyGid = await CreateCompanyAndGetGidAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            $"/api/company-manage/{companyGid}/building",
            ValidBuildingRequest("E"));
        var created = await createResponse.ReadWithJson<CreateCompanyBuildingResult>(_output);

        var updateRequest = new UpdateCompanyBuildingRequest("Blok E Updated", "ul. Nowa 5", "Gdańsk", "80-001", "PL");
        var response = await client.PutAsJsonAsync(
            $"/api/company-manage/{companyGid}/building/{created!.BuildingId}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_UpdateBuilding_NotFound_Returns_NotFound()
    {
        var client     = _factory.CreateClient().WithAuth(UserRole.Admin);
        var companyGid = await CreateCompanyAndGetGidAsync(client);

        var updateRequest = new UpdateCompanyBuildingRequest("Blok X", "ul. Nieistniejaca 1", "Warszawa", "00-001", "PL");
        var response = await client.PutAsJsonAsync(
            $"/api/company-manage/{companyGid}/building/{Guid.NewGuid()}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_UpdateBuilding_WrongCompany_Returns_NotFound()
    {
        var client     = _factory.CreateClient().WithAuth(UserRole.Admin);
        var company1Gid = await CreateCompanyAndGetGidAsync(client);
        var company2Gid = await CreateCompanyAndGetGidAsync(client);

        // Budynek należy do company1
        var createResponse = await client.PostAsJsonAsync(
            $"/api/company-manage/{company1Gid}/building",
            ValidBuildingRequest("F"));
        var created = await createResponse.ReadWithJson<CreateCompanyBuildingResult>(_output);

        // Próba aktualizacji pod company2 — powinno zwrócić 404
        var updateRequest = new UpdateCompanyBuildingRequest("Blok F Hacked", "ul. Wspólna 3", "Poznań", "60-001", "PL");
        var response = await client.PutAsJsonAsync(
            $"/api/company-manage/{company2Gid}/building/{created!.BuildingId}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_GetBuildings_After_Create_Returns_Item()
    {
        var client     = _factory.CreateClient().WithAuth(UserRole.Admin);
        var companyGid = await CreateCompanyAndGetGidAsync(client);
        var suffix     = Guid.NewGuid().ToString("N")[..8];

        await client.PostAsJsonAsync(
            $"/api/company-manage/{companyGid}/building",
            new CreateCompanyBuildingRequest("Blok " + suffix, "ul. Listowa 7", "Łódź", "90-001", "PL"));

        var listResponse = await client.GetAsync($"/api/company-manage/{companyGid}/building");
        var paged = await listResponse.ReadWithJson<PagedResult<CompanyBuildingListItemDto>>(_output);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Contains(paged!.Items, b => b.Name == "Blok " + suffix);
    }
}