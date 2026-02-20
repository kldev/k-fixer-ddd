namespace K.Fixer.Application.IAM.Login;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string role, string fullName, Guid? companyId);
    
}