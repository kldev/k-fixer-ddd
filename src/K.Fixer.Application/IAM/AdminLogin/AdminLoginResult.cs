namespace K.Fixer.Application.IAM.AdminLogin;

public record AdminLoginResult
(
    Guid  AdminId,
    string FullName,
    string Token
);