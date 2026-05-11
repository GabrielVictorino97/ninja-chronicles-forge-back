namespace KageNoTessen.Contracts.Auth;

public record RegisterRequest(string Email, string Name, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string AccessToken, string RefreshToken);
public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);

public record UserDto(
    Guid Id, string Email, string Name, string Role,
    DateTime CreatedAt, DateTime? LastLoginAt);
