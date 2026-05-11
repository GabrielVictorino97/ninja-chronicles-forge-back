namespace NarutoPlayers.Domain;

public class Battle : AuditableEntity
{
    public BattleStatus Status { get; private set; } = BattleStatus.Ongoing;
    public int Turn { get; private set; }
    public bool IsPlayerTurn { get; private set; } = true;
    public BattleType Type { get; private set; }

    public ICollection<BattleParticipant> Participants { get; private set; } = new List<BattleParticipant>();
    public ICollection<BattleLog> Logs { get; private set; } = new List<BattleLog>();

    private Battle() { }

    public static Battle Create(BattleType type)
        => new() { Type = type };

    public void AddLog(string message, BattleActorSide actor, int? damage = null)
        => Logs.Add(BattleLog.Create(Id, Turn, actor, message, damage));

    public void NextTurn() { Turn++; IsPlayerTurn = !IsPlayerTurn; }
    public void End(BattleStatus status) => Status = status;
}

public enum BattleType { PvE, PvP, Boss, Arena }

public class BattleParticipant : BaseEntity
{
    public Guid BattleId { get; private set; }
    public Guid? CharacterId { get; set; }
    public BattleActorSide Side { get; private set; }
    public string Name { get; private set; } = null!;
    public string Avatar { get; private set; } = null!;
    public int Hp { get; set; }
    public int HpMax { get; set; }
    public int Chakra { get; set; }
    public int ChakraMax { get; set; }
    public int Level { get; set; }
    public int PhysicalAttack { get; set; }
    public int NinjutsuAttack { get; set; }
    public int GenjutsuAttack { get; set; }
    public int PhysicalDefense { get; set; }
    public int SpiritualDefense { get; set; }
    public int MentalResistance { get; set; }
    public int Initiative { get; set; }
    public int CritChance { get; set; }
    public int Dodge { get; set; }
    public int Precision { get; set; }
    public bool Defending { get; set; }

    public Battle Battle { get; private set; } = null!;

    private BattleParticipant() { }

    public static BattleParticipant Create(Guid battleId, BattleActorSide side, string name, string avatar,
        int hpMax, int chakraMax, int level, int pa, int na, int ga, int pd, int sd, int mr, int init, int crit, int dodge, int prec)
        => new()
        {
            BattleId = battleId, Side = side, Name = name, Avatar = avatar,
            Hp = hpMax, HpMax = hpMax, Chakra = chakraMax, ChakraMax = chakraMax, Level = level,
            PhysicalAttack = pa, NinjutsuAttack = na, GenjutsuAttack = ga,
            PhysicalDefense = pd, SpiritualDefense = sd, MentalResistance = mr,
            Initiative = init, CritChance = crit, Dodge = dodge, Precision = prec
        };
}

public class BattleLog : BaseEntity
{
    public Guid BattleId { get; private set; }
    public int Turn { get; private set; }
    public BattleActorSide Actor { get; private set; }
    public string Message { get; private set; } = null!;
    public int? Damage { get; private set; }

    public Battle Battle { get; private set; } = null!;

    private BattleLog() { }

    public static BattleLog Create(Guid battleId, int turn, BattleActorSide actor, string message, int? damage = null)
        => new() { BattleId = battleId, Turn = turn, Actor = actor, Message = message, Damage = damage };
}

public class Item : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public ItemType Type { get; private set; }
    public ItemRarity Rarity { get; private set; }
    public string Description { get; private set; } = null!;
    public int Price { get; private set; }
    public string Icon { get; private set; } = null!;
    public bool Sellable { get; set; } = true;
    public bool Equippable { get; set; }
    public bool Consumable { get; set; }
    public int MinLevel { get; set; } = 1;
    public Graduation MinGraduation { get; set; } = Graduation.Estudante;
    public bool Active { get; set; } = true;

    public ICollection<InventoryItem> Inventories { get; private set; } = new List<InventoryItem>();

    private Item() { }

    public static Item Create(string name, ItemType type, ItemRarity rarity, string description, int price, string icon)
        => new() { Name = name, Type = type, Rarity = rarity, Description = description, Price = price, Icon = icon };
}

public class InventoryItem : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public Guid ItemId { get; private set; }
    public int Quantity { get; private set; } = 1;
    public bool Equipped { get; private set; }
    public EquipSlot? Slot { get; private set; }

    public Character Character { get; private set; } = null!;
    public Item Item { get; private set; } = null!;

    private InventoryItem() { }

    public static InventoryItem Acquire(Guid characterId, Guid itemId, int quantity = 1)
        => new() { CharacterId = characterId, ItemId = itemId, Quantity = quantity };

    public void AddQuantity(int qty) => Quantity += qty;
    public void RemoveQuantity(int qty) => Quantity = Math.Max(0, Quantity - qty);
    public void Equip(EquipSlot slot) { Equipped = true; Slot = slot; }
    public void Unequip() { Equipped = false; Slot = null; }
}

public class Wallet : BaseEntity
{
    public Guid CharacterId { get; private set; }
    public int Balance { get; private set; } = 100;
    public Character Character { get; private set; } = null!;
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    private Wallet() { }

    public static Wallet Create(Guid characterId) => new() { CharacterId = characterId };

    public bool Debit(int amount) { if (Balance < amount) return false; Balance -= amount; return true; }
    public void Credit(int amount) => Balance += amount;
}

public class Transaction : BaseEntity
{
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public int Amount { get; private set; }
    public string Description { get; private set; } = null!;
    public Wallet Wallet { get; private set; } = null!;

    private Transaction() { }

    public static Transaction Create(Guid walletId, TransactionType type, int amount, string description)
        => new() { WalletId = walletId, Type = type, Amount = amount, Description = description };
}
