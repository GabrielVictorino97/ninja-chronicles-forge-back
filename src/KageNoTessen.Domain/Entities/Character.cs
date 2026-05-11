namespace KageNoTessen.Domain;

public class Character : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Avatar { get; private set; } = null!;
    public Guid VillageId { get; private set; }
    public Guid ClanId { get; private set; }
    public Graduation Graduation { get; private set; } = Graduation.Estudante;
    public int Level { get; private set; } = 1;
    public int Xp { get; private set; }
    public int XpToNext { get; private set; } = 100;
    public int Hp { get; private set; }
    public int HpMax { get; private set; }
    public int Chakra { get; private set; }
    public int ChakraMax { get; private set; }
    public int Energy { get; private set; }
    public int EnergyMax { get; private set; } = 100;
    public int Ryous { get; private set; } = 100;
    public int Power { get; private set; }
    public int UnspentPoints { get; private set; } = 10;
    public bool Active { get; set; } = true;

    public CharacterAttributes Attributes { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Village Village { get; private set; } = null!;
    public BloodlineClan Clan { get; private set; } = null!;
    public ICollection<CharacterElement> CharacterElements { get; private set; } = new List<CharacterElement>();
    public ICollection<CharacterJutsu> CharacterJutsus { get; private set; } = new List<CharacterJutsu>();
    public ICollection<InventoryItem> Inventory { get; private set; } = new List<InventoryItem>();
    public ICollection<CharacterMission> Missions { get; private set; } = new List<CharacterMission>();
    public ICollection<CharacterAchievement> Achievements { get; private set; } = new List<CharacterAchievement>();
    public Wallet Wallet { get; private set; } = null!;

    private Character() { }

    public static Character Create(Guid userId, string name, string avatar, Guid villageId, Guid clanId)
    {
        var c = new Character
        {
            UserId = userId, Name = name, Avatar = avatar,
            VillageId = villageId, ClanId = clanId
        };
        c.Attributes = CharacterAttributes.CreateDefault(c.Id);
        c.ApplyDerivedAttributes();
        c.Hp = c.HpMax;
        c.Chakra = c.ChakraMax;
        c.Energy = c.EnergyMax;
        return c;
    }

    public void ApplyDerivedAttributes()
    {
        var a = Attributes;
        HpMax = 100 + a.Vitality * 12 + Level * 20;
        ChakraMax = 80 + a.Chakra * 10 + Level * 15;
        Power = a.Taijutsu + a.Ninjutsu + a.Genjutsu + a.Intelligence + a.Vitality + a.Chakra + a.Agility + a.Luck + Level * 10;
    }

    public void AddXp(int amount)
    {
        Xp += amount;
        while (Xp >= XpToNext) { LevelUp(); }
    }

    private void LevelUp()
    {
        Xp -= XpToNext;
        Level++;
        XpToNext = 100 + Level * 50;
        UnspentPoints += 3;
        ApplyDerivedAttributes();
        Hp = HpMax;
        Chakra = ChakraMax;
    }

    public void Graduate()
    {
        Graduation = Graduation switch
        {
            Domain.Graduation.Estudante => Domain.Graduation.Genin,
            Domain.Graduation.Genin => Domain.Graduation.Chunin,
            Domain.Graduation.Chunin => Domain.Graduation.TokubetsuJounin,
            Domain.Graduation.TokubetsuJounin => Domain.Graduation.Jounin,
            Domain.Graduation.Jounin => Domain.Graduation.ANBU,
            Domain.Graduation.ANBU => Domain.Graduation.Sannin,
            Domain.Graduation.Sannin => Domain.Graduation.Lendario,
            Domain.Graduation.Kage => Domain.Graduation.Lendario,
            _ => Graduation
        };
    }

    public bool CanGraduate() => Graduation switch
    {
        Domain.Graduation.Estudante => Level >= 5,
        Domain.Graduation.Genin => Level >= 15,
        Domain.Graduation.Chunin => Level >= 25,
        Domain.Graduation.TokubetsuJounin => Level >= 35,
        Domain.Graduation.Jounin => Level >= 45,
        Domain.Graduation.ANBU => Level >= 75,
        Domain.Graduation.Sannin => Level >= 100,
        Domain.Graduation.Kage => Level >= 100,
        _ => false
    };

    public void SetKage() => Graduation = Graduation.Kage;

    public void AddRyous(int amount) => Ryous += amount;
    public bool SpendRyous(int amount) { if (Ryous < amount) return false; Ryous -= amount; return true; }
    public bool SpendEnergy(int amount) { if (Energy < amount) return false; Energy -= amount; return true; }
    public void RestoreEnergy(int amount) => Energy = Math.Min(EnergyMax, Energy + amount);
    public void TakeDamage(int damage) => Hp = Math.Max(0, Hp - damage);
    public void Heal(int amount) => Hp = Math.Min(HpMax, Hp + amount);
    public bool SpendChakra(int amount) { if (Chakra < amount) return false; Chakra -= amount; return true; }
    public void SpendPoints(int amount) { if (amount > UnspentPoints) throw new InvalidOperationException("Not enough points"); UnspentPoints -= amount; }
}

public class CharacterHunt : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public DateTime StartTime { get; private set; } = DateTime.UtcNow;
    public DateTime EndTime { get; private set; }
    public int HuntLevel { get; private set; }
    public int DurationMinutes { get; private set; }
    public int XpReward { get; private set; }
    public int RyousReward { get; private set; }
    public bool Completed { get; private set; }

    public Character Character { get; private set; } = null!;

    private CharacterHunt() { }

    public static CharacterHunt Start(Guid characterId, int huntLevel, int durationMinutes, int xpReward, int ryousReward)
        => new()
        {
            CharacterId = characterId,
            HuntLevel = huntLevel,
            DurationMinutes = durationMinutes,
            XpReward = xpReward,
            RyousReward = ryousReward,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(durationMinutes),
        };

    public bool IsExpired() => DateTime.UtcNow >= EndTime;
    public void Complete() => Completed = true;
}

public class CharacterElement : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public ElementAffinity Element { get; private set; }
    public DateTime LearnedAt { get; private set; } = DateTime.UtcNow;

    public Character Character { get; private set; } = null!;

    private CharacterElement() { }

    public static CharacterElement Learn(Guid characterId, ElementAffinity element)
        => new() { CharacterId = characterId, Element = element };
}

public class CharacterAttributes : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public int Taijutsu { get; set; } = 5;
    public int Ninjutsu { get; set; } = 5;
    public int Genjutsu { get; set; } = 5;
    public int Intelligence { get; set; } = 5;
    public int Vitality { get; set; } = 5;
    public int Chakra { get; set; } = 5;
    public int Agility { get; set; } = 5;
    public int Luck { get; set; } = 5;

    public Character Character { get; private set; } = null!;

    private CharacterAttributes() { }

    public static CharacterAttributes CreateDefault(Guid characterId)
        => new() { CharacterId = characterId };
}
