namespace KageNoTessen.Domain;

public class PlayerClan : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Tag { get; private set; } = null!;
    public int Level { get; private set; } = 1;
    public int Xp { get; private set; }
    public int XpToNext { get; private set; } = 100;
    public int Ranking { get; private set; }

    public ICollection<PlayerClanMember> Members { get; private set; } = new List<PlayerClanMember>();
    public ICollection<ClanWallPost> Wall { get; private set; } = new List<ClanWallPost>();

    private PlayerClan() { }

    public static PlayerClan Create(string name, string tag, Guid leaderId, string leaderName)
    {
        var clan = new PlayerClan { Name = name, Tag = tag };
        clan.Members.Add(PlayerClanMember.Create(clan.Id, leaderId, leaderName, ClanRole.Leader));
        return clan;
    }

    public void AddXp(int amount)
    {
        Xp += amount;
        while (Xp >= XpToNext) { Xp -= XpToNext; Level++; XpToNext = 100 + Level * 75; }
    }
}

public class PlayerClanMember : BaseEntity
{
    public Guid ClanId { get; private set; }
    public Guid CharacterId { get; private set; }
    public string Name { get; private set; } = null!;
    public int Level { get; private set; }
    public ClanRole Role { get; private set; }
    public int Donations { get; private set; }

    public PlayerClan Clan { get; private set; } = null!;

    private PlayerClanMember() { }

    public static PlayerClanMember Create(Guid clanId, Guid characterId, string name, ClanRole role)
        => new() { ClanId = clanId, CharacterId = characterId, Name = name, Role = role };

    public void Donate(int amount) => Donations += amount;
    public void SetRole(ClanRole role) => Role = role;
}

public class ClanWallPost : BaseEntity
{
    public Guid ClanId { get; private set; }
    public string Author { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public PlayerClan Clan { get; private set; } = null!;

    private ClanWallPost() { }

    public static ClanWallPost Create(Guid clanId, string author, string message)
        => new() { ClanId = clanId, Author = author, Message = message };
}

public class WorldLocation : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public LocationType Type { get; private set; }
    public Graduation GraduationRequired { get; private set; } = Graduation.Estudante;
    public string Description { get; private set; } = null!;
    public string[] Enemies { get; set; } = Array.Empty<string>();

    private WorldLocation() { }

    public static WorldLocation Create(string name, LocationType type, string description)
        => new() { Name = name, Type = type, Description = description };
}

public class GameEvent : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public EventType Type { get; private set; }
    public EventStatus Status { get; private set; } = EventStatus.Scheduled;
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public double XpMultiplier { get; set; } = 1.0;
    public double DropMultiplier { get; set; } = 1.0;
    public string[] Rewards { get; set; } = Array.Empty<string>();
    public string Banner { get; set; } = null!;

    private GameEvent() { }

    public static GameEvent Create(string name, string description, EventType type, DateTime startsAt, DateTime endsAt)
        => new() { Name = name, Description = description, Type = type, StartsAt = startsAt, EndsAt = endsAt };
}

public class Achievement : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Icon { get; private set; } = null!;

    private Achievement() { }

    public static Achievement Create(string name, string description, string icon)
        => new() { Name = name, Description = description, Icon = icon };
}

public class CharacterAchievement : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid AchievementId { get; private set; }
    public DateTime UnlockedAt { get; private set; } = DateTime.UtcNow;

    public Character Character { get; private set; } = null!;
    public Achievement Achievement { get; private set; } = null!;

    private CharacterAchievement() { }

    public static CharacterAchievement Unlock(Guid characterId, Guid achievementId)
        => new() { CharacterId = characterId, AchievementId = achievementId };
}

public class AuditLog : BaseEntity
{
    public string User { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string Entity { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public string Ip { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    private AuditLog() { }

    public static AuditLog Create(string user, string action, string entity, string entityId, string ip, string description)
        => new() { User = user, Action = action, Entity = entity, EntityId = entityId, Ip = ip, Description = description };
}
