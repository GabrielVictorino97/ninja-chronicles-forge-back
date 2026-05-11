namespace KageNoTessen.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string email, string role);
    string GenerateRefreshToken();
    (Guid userId, string email, string role) ValidateToken(string token);
}

public interface IClock
{
    DateTime UtcNow { get; }
}
