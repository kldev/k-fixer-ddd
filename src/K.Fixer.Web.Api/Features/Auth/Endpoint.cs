using System.Security.Claims;

using Common.Toolkit.Extensions;
using Common.Toolkit.ResultPattern;

using K.Fixer.Application.IAM.AdminLogin;
using K.Fixer.Application.IAM.Login;
using K.Fixer.Web.Api.Extensions;

using Microsoft.AspNetCore.Mvc;

namespace K.Fixer.Web.Api.Features.Auth;

public static class Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGroup("/api/auth").MapGroupEndpoints().WithTags("Auth");
    }

    private static RouteGroupBuilder MapGroupEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/session-info",
                (ClaimsPrincipal principal) => TypedResults.Ok(principal.ToDictionary()))
            .RequireAuthorization();

        builder.MapPost("/login", HandleLogin).AllowAnonymous();

        return builder;
    }

    private static async Task<IResult> HandleLogin(LoginHandler loginHandler, AdminLoginHandler adminLoginHandler,
        [FromBody] LoginRequest request)
    {
        if (request.Username.Contains('@'))
        {
            var userResult = await loginHandler.HandleAsync(new LoginCommand(request.Username, request.Password));
            if (userResult is Success<LoginResult> { Value: var user })
                return TypedResults.Ok(new SessionResponse(user.Role, user.UserId, user.FullName, user.Token));

            return userResult.ToApiResult();
        }
        else
        {
            var adminResult = await adminLoginHandler.HandleAsync(new AdminLoginCommand(request.Username, request.Password));
            if (adminResult is Success<AdminLoginResult> { Value: var admin })
                return TypedResults.Ok(new SessionResponse("Admin", admin.AdminId, admin.FullName, admin.Token));

            return adminResult.ToApiResult();
        }
    }
}