using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Common.Toolkit;

using K.Fixer.Application.IAM.Login;
using K.Fixer.Domain.IAM.Aggregates.User;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace K.Fixer.Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    // ReSharper disable once ConvertToPrimaryConstructor
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string email, string role, string fullName, Guid? companyId)
    {
        Claim[] claims =
        [
            new(AppClaims.UserId, userId.ToString()),
            new(AppClaims.Role, role),
            new(AppClaims.Email, email), new(AppClaims.FullName, fullName),
            new(AppClaims.CompanyId, companyId?.ToString() ?? GuidUtil.NonEmptyZero.ToString())
        ];

        return CreateToken(claims);
    }


    private string CreateToken(Claim[] claims)
    { 
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        
        var expires = DateTime.UtcNow.AddHours(6);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;

    }
}