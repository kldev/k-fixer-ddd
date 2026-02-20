using System.Net;
using System.Net.Http.Json;

using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Api.Features.Auth;
using K.Fixer.Web.Api.Tests.Extensions;
using K.Fixer.Web.Api.Tests.Infrastructure;
using K.Fixer.Web.Seed.FixedValue;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace K.Fixer.Web.Api.Tests.Endpoints;


[Collection(PostgresCollection.Name)]
public class AuthEndpointTest
{
    private const string Password = "fixer777";
    private const string BcryptPassword = "$2a$12$7eIE62SbQJYX6hwbVhy4JOQLO4ZbsbotCpKt7HE3gwz1LSKy4CwCu";

    private readonly ITestOutputHelper _output;
    private readonly KFixerWebApplicationFactory _factory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AuthEndpointTest(PostgresFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _factory = new KFixerWebApplicationFactory(fixture.ConnectionString);
    }

    // --- Admin ---

    [Fact]
    public async Task Admin_Login_Returns_Ok_With_AdminRole()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await response.ReadWithJson<SessionResponse>(_output);
        Assert.NotNull(session);
        Assert.Equal(UserRole.Admin.Value, session.Role);
        Assert.NotEmpty(session.Token);
    }

    [Fact]
    public async Task Admin_Login_WrongPassword_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "wrongpassword"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Login_NotFound_Returns_NotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ghostadmin", Password));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Acme users ---

    [Theory]
    [InlineData("victoria@acme.com", "CompanyAdmin")]
    [InlineData("anna@acme.com", "Technician")]
    [InlineData("daniel@acme.com", "Resident")]
    public async Task User_Login_AcmeUsers_Returns_Ok_With_CorrectRole(string email, string expectedRole)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await response.ReadWithJson<SessionResponse>(_output);
        Assert.NotNull(session);
        Assert.Equal(expectedRole, session.Role);
        Assert.NotEmpty(session.Token);
    }

    [Fact]
    public async Task User_Login_WrongPassword_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("victoria@cme.com", "wrongpassword"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task User_Login_NotFound_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ghost@cme.com", Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task User_Login_NotActive_Returns_BadRequest()
    {
        var email = await CreateTestUser(isActive: false, isLocked: false);
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task User_Login_Locked_Returns_BadRequest()
    {
        var email = await CreateTestUser(isActive: true, isLocked: true);
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Validation ---

    [Fact]
    public async Task Login_EmptyUsername_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("", Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmptyPassword_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Helpers ---

    private async Task<string> CreateTestUser(bool isActive, bool isLocked)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var companyId = await db.CompaniesReads
          .Where(c => c.Id == CompanyGuids.Acme)
          .Select(c => c.Id)
          .FirstAsync();

        var email = $"t{Guid.NewGuid().ToString("N")[..10]}@cme.com";

        var user = User.Create(email, Password, "Test user", UserRole.Resident.Value, companyId);
        
        db.Users.Add(user.Value!);
        if (isLocked)
        {
            user.Value!.Lock();
        }

        if (!isActive)
        {
            user.Value!.Deactivate();
        }

        await db.SaveChangesAsync();
        return email;
    }
}