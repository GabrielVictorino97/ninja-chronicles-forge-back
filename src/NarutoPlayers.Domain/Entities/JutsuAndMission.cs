namespace NarutoPlayers.Domain;

public class Jutsu : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public JutsuType Type { get; set; }
    public ElementAffinity? Element { get; set; }
    public int ChakraCost { get; set; }
    public int Cooldown { get; set; }
    public int BaseDamage { get; set; }
    public string Description { get; set; } = null!;
    public int MinLevel { get; set; } = 1;
    public Graduation MinGraduation { get; set; } = Graduation.Estudante;
    public string? ClanRequirement { get; set; }
    public ElementAffinity? ElementRequirement { get; set; }
    public bool Pvp { get; set; } = true;
    public bool Pve { get; set; } = true;
    public bool Active { get; set; } = true;

    public ICollection<CharacterJutsu> CharacterJutsus { get; private set; } = new List<CharacterJutsu>();

    private Jutsu() { }

    public static Jutsu Create(string name, JutsuType type, int chakraCost, int cooldown, int baseDamage, string description)
        => new() { Name = name, Type = type, ChakraCost = chakraCost, Cooldown = cooldown, BaseDamage = baseDamage, Description = description };
}

public class CharacterJutsu : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid JutsuId { get; private set; }
    public bool Equipped { get; private set; }
    public int Level { get; private set; } = 1;
    public DateTime LearnedAt { get; private set; } = DateTime.UtcNow;

    public Character Character { get; private set; } = null!;
    public Jutsu Jutsu { get; private set; } = null!;

    private CharacterJutsu() { }

    public static CharacterJutsu Learn(Guid characterId, Guid jutsuId)
        => new() { CharacterId = characterId, JutsuId = jutsuId };

    public void Equip() => Equipped = true;
    public void Unequip() => Equipped = false;
    public void Upgrade() => Level++;
    public int GetBaseDamage() => Jutsu.BaseDamage + (Level - 1) * 10;
}

public class Mission : AuditableEntity
{
    public string Title { get; private set; } = null!;
    public string Description { get; set; } = null!;
    public Rank Rank { get; private set; }
    public MissionType Type { get; set; }
    public int EnergyCost { get; set; }
    public int XpReward { get; set; }
    public int RyousReward { get; set; }
    public int MinLevel { get; set; } = 1;
    public Graduation MinGraduation { get; set; } = Graduation.Estudante;
    public int DurationMinutes { get; set; }
    public int CooldownMinutes { get; set; }
    public bool Repeatable { get; set; } = true;
    public bool Active { get; set; } = true;
    public string[] Drops { get; set; } = Array.Empty<string>();
    public string[] Enemies { get; set; } = Array.Empty<string>();

    public ICollection<CharacterMission> CharacterMissions { get; private set; } = new List<CharacterMission>();

    private Mission() { }

    public static Mission Create(string title, Rank rank, int energyCost, int xpReward, int ryousReward)
        => new() { Title = title, Rank = rank, EnergyCost = energyCost, XpReward = xpReward, RyousReward = ryousReward };
}

public class CharacterMission : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid MissionId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool Completed { get; private set; }

    public Character Character { get; private set; } = null!;
    public Mission Mission { get; private set; } = null!;

    private CharacterMission() { }

    public static CharacterMission Start(Guid characterId, Guid missionId)
        => new() { CharacterId = characterId, MissionId = missionId, StartedAt = DateTime.UtcNow };

    public void Complete() { CompletedAt = DateTime.UtcNow; Completed = true; }
}
