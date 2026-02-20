using Common.Toolkit.ResultPattern;

using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.IAM.Repositories;

namespace K.Fixer.Application.IAM.Login;

public sealed class LoginHandler
{
    private readonly IUserRepository  _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public LoginHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository  = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand command,
        CancellationToken ct = default)
    {

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return new ValidationError("Login.InvalidPasswordOrEmail", "Invalid email or password.");
        
        var user = await _userRepository.GetByEmail(emailResult.Value!, ct);
        if (user is null)
            return new ValidationError("Login.invalidPasswordOrEmail", "Invalid email or password.");
        
        if (!user.CanLogin())
            return new ValidationError("Login.isLocker", "Account is locked. Contact the service administrator.");
        
        var verifyResult = user.VerifyPassword(command.Password);
        if (verifyResult.IsFailure)
            return new ValidationError("Login.invalidPasswordOrEmail", "Invalid email or password.");
        
        var token = _jwtTokenService.GenerateToken(
            user.Id, user.Email.Value, user.Role.Value,  user.FullName.Value, user.CompanyId);

        return new LoginResult(
            user.Id,
            user.FullName.Value,
            user.Role.Value,
            user.CompanyId,
            token);
    }
}