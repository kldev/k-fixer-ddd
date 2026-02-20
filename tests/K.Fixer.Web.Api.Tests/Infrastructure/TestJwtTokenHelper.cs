using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Infrastructure.Auth;

using Microsoft.IdentityModel.Tokens;

namespace K.Fixer.Web.Api.Tests.Infrastructure;

public static class TestJwtTokenHelper
{
    private const string Key = "SuperSecretKeyForDemoPurposesOnly-AtLeast32Bytes!";
    private const string Issuer = "KFixer";
    private const string Audience = "KFixer";

    public static string GenerateToken(UserRole role, string? companyId = null)
    {
        var claims = new List<Claim>
        {
            new(AppClaims.FullName, $"test-{role.ToString().ToLower()}"),
            new(AppClaims.Role, role.ToString()),
            new(AppClaims.Email, $"test-{role.ToString().ToLower()}@fixer.local"),
        };

        if (companyId is not null)
            claims.Add(new Claim(AppClaims.CompanyId, companyId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static HttpClient WithAuth(this HttpClient client, UserRole role, string? companyId = null)
    {
        var token = GenerateToken(role, companyId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}