using Common.Toolkit.ResultPattern;

using K.Fixer.Application.IAM.Login;
using K.Fixer.Domain.IAM.Aggregates.AdminUser;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.IAM.Repositories;

namespace K.Fixer.Application.IAM.AdminLogin;

public class AdminLoginHandler
{
    private readonly IAdminUserRepository  _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AdminLoginHandler(
        IAdminUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository  = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AdminLoginResult>> HandleAsync(
        AdminLoginCommand command,
        CancellationToken ct = default)
    {

        var usernameResult = Username.Create(command.Username);
        if (usernameResult.IsFailure)
            return new ValidationError("Login.InvalidPasswordOrEmail", "Invalid username or password.");

        var user = await _userRepository.GetByUsername(usernameResult.Value!, ct);
        if (user is null)
            return new NotFoundError("Login.userNotFound", "Invalid username.");

        var verifyResult = user.VerifyPassword(command.Password);
        if (verifyResult.IsFailure)
            return new ValidationError("Login.invalidPasswordOrEmail", "Invalid email or password.");

        var token = _jwtTokenService.GenerateToken(
            user.Id, "", UserRole.Admin.Value, user.FullName.Value, null);

        return new AdminLoginResult(
            user.Id, user.FullName.Value, token
        );
    }
}