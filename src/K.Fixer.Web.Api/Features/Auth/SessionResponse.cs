namespace K.Fixer.Web.Api.Features.Auth;

public record SessionResponse(string Role, Guid UserGid, string UserName, string Token);