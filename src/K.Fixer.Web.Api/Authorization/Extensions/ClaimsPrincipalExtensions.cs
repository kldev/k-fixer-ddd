using System.Security.Claims;

using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Infrastructure.Auth;

namespace K.Fixer.Web.Api.Authorization.Extensions;


public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
        => Guid.Parse(principal.FindFirstValue(AppClaims.UserId)
                      ?? throw new UnauthorizedAccessException("Missing user ID claim."));

    public static Guid? GetCompanyId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(AppClaims.CompanyId);
        return !string.IsNullOrEmpty(value) ? Guid.Parse(value) : null;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(AppClaims.Email);
        return !string.IsNullOrEmpty(value) ? value : null;
    }

    public static UserRole GetRole(this ClaimsPrincipal principal)
        => UserRole.Create(principal.FindFirstValue(AppClaims.Role)).Value!;

    public static bool IsAdmin(this ClaimsPrincipal principal)
        => principal.GetRole() == UserRole.Admin;
}