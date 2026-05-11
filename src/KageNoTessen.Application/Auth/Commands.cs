using FluentValidation;
using MediatR;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Auth;
using KageNoTessen.Domain;

namespace KageNoTessen.Application.Auth;

public record RegisterCommand(string Email, string Name, string Password) : IRequest<AuthResponse>;
public record LoginCommand(string Email, string Password, string Ip, string Device) : IRequest<AuthResponse>;
public record RefreshCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse>;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator(IUserRepository users)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(64);
        RuleFor(x => x.Password).MinimumLength(4).MaximumLength(128);
        RuleFor(x => x.Email).MustAsync(async (email, ct) => !await users.EmailExistsAsync(email, ct))
            .WithMessage("Email already registered");
    }
}

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class AuthHandler :
    IRequestHandler<RegisterCommand, AuthResponse>,
    IRequestHandler<LoginCommand, AuthResponse>,
    IRequestHandler<RefreshCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtService _jwt;
    private readonly IClock _clock;
    private const int RefreshTokenDays = 7;

    public AuthHandler(IUserRepository users, IRefreshTokenRepository refreshTokens, IJwtService jwt, IClock clock)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<AuthResponse> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);
        var user = User.Create(cmd.Email, cmd.Name, hash);
        await _users.AddAsync(user, ct);

        var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _jwt.GenerateRefreshToken();
        await _refreshTokens.AddAsync(
            RefreshToken.Create(user.Id, refreshToken, _clock.UtcNow.AddDays(RefreshTokenDays)), ct);

        return new AuthResponse(accessToken, refreshToken, ToDto(user));
    }

    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(cmd.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (user.Status == UserStatus.Banned)
            throw new UnauthorizedAccessException("Account is banned");

        if (!BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        user.RecordLogin(cmd.Ip);
        await _users.UpdateAsync(user, ct);

        var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _jwt.GenerateRefreshToken();
        await _refreshTokens.AddAsync(
            RefreshToken.Create(user.Id, refreshToken, _clock.UtcNow.AddDays(RefreshTokenDays)), ct);

        return new AuthResponse(accessToken, refreshToken, ToDto(user));
    }

    public async Task<AuthResponse> Handle(RefreshCommand cmd, CancellationToken ct)
    {
        Guid userId;
        string email, role;
        try
        {
            (userId, email, role) = _jwt.ValidateToken(cmd.AccessToken);
        }
        catch { throw new UnauthorizedAccessException("Invalid token"); }

        var stored = await _refreshTokens.GetByTokenAsync(cmd.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (!stored.IsValid() || stored.UserId != userId)
            throw new UnauthorizedAccessException("Invalid refresh token");

        stored.Revoke();
        await _refreshTokens.UpdateAsync(stored, ct);

        var newAccess = _jwt.GenerateAccessToken(userId, email, role);
        var newRefresh = _jwt.GenerateRefreshToken();
        await _refreshTokens.AddAsync(
            RefreshToken.Create(userId, newRefresh, _clock.UtcNow.AddDays(RefreshTokenDays)), ct);

        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        return new AuthResponse(newAccess, newRefresh, ToDto(user));
    }

    private static UserDto ToDto(User u) => new(
        u.Id, u.Email, u.Name, u.Role.ToString(),
        u.CreatedAt, u.LastLoginAt);
}
