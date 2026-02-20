using K.Fixer.Domain.IAM.Aggregates.AdminUser;

namespace K.Fixer.Application.IAM.AdminLogin;

public record AdminLoginCommand(
    string Username, 
    string Password
);
