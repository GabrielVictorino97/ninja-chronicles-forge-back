namespace KageNoTessen.Domain;

public class User : AuditableEntity
{
    public string Email { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; } = UserRole.Player;
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<UserLoginHistory> LoginHistory { get; private set; } = new List<UserLoginHistory>();
    public ICollection<Character> Characters { get; private set; } = new List<Character>();
    public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();

    private User() { }

    public static User Create(string email, string name, string passwordHash)
        => new() { Email = email.ToLowerInvariant(), Name = name, PasswordHash = passwordHash };

    public void RecordLogin(string ip) { LastLoginAt = DateTime.UtcNow; LastLoginIp = ip; }
    public void UpdatePassword(string hash) => PasswordHash = hash;
    public void Ban() => Status = UserStatus.Banned;
    public void Unban() => Status = UserStatus.Active;
    public void SetRole(UserRole role) => Role = role;
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool Revoked { get; private set; }
    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
        => new() { UserId = userId, Token = token, ExpiresAt = expiresAt };

    public void Revoke() => Revoked = true;
    public bool IsValid() => !Revoked && ExpiresAt > DateTime.UtcNow;
}

public class UserLoginHistory : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Ip { get; private set; } = null!;
    public string Device { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private UserLoginHistory() { }

    public static UserLoginHistory Create(Guid userId, string ip, string device)
        => new() { UserId = userId, Ip = ip, Device = device };
}

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public NotificationType Type { get; private set; }
    public bool Read { get; private set; }
    public User User { get; private set; } = null!;

    private Notification() { }

    public static Notification Create(Guid userId, string title, string description, NotificationType type)
        => new() { UserId = userId, Title = title, Description = description, Type = type };

    public void MarkRead() => Read = true;
}
