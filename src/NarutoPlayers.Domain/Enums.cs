namespace NarutoPlayers.Domain;

public enum Rank { D, C, B, A, S }

public enum Graduation
{
    Estudante, Genin, Chunin, TokubetsuJounin, Jounin, ANBU, Sannin, Kage, Lendario
}

public enum ElementAffinity
{
    Katon, Suiton, Doton, Fuuton, Raiton,
    Mokuton, Hyoton, Yoton, Jiton, Bakuton,
    Yin, Yang, YinYang
}

public enum JutsuType
{
    Taijutsu, Ninjutsu, Genjutsu, Fuinjutsu, IryoNinjutsu,
    Senjutsu, Doujutsu, Kinjutsu, Kuchiyose, KekkeiGenkai
}

public enum ItemType
{
    Weapon, Armor, Accessory, Tool, Consumable, Summon
}

public enum ItemRarity
{
    Common, Uncommon, Rare, Epic, Legendary
}

public enum EquipSlot
{
    Weapon, Armor, Accessory1, Accessory2, Tool, Summon
}

public enum BattleStatus
{
    Ongoing, Victory, Defeat, Fled
}

public enum BattleActionType
{
    Basic, Defend, Item, Flee, Jutsu
}

public enum BattleActorSide
{
    Player, Enemy, System
}

public enum ClanRole
{
    Leader, SubLeader, Officer, Member, Recruit
}

public enum MissionType
{
    Delivery, Patrol, Escort, Investigation, Capture,
    VillageDefense, Assassination, Infiltration, Rescue,
    Training, Boss, Story, Daily, Weekly, Clan
}

public enum LocationType
{
    Pais, Regiao, Santuario, Esconderijo
}

public enum EventType
{
    Boss, Torneio, Invasao, Bonus, Historia
}

public enum EventStatus
{
    Scheduled, Ongoing, Ended
}

public enum UserRole
{
    Player, Moderator, Admin, SuperAdmin
}

public enum UserStatus
{
    Active, Banned, Blocked, Pending
}

public enum NotificationType
{
    Info, Success, Warning, Battle, Mission
}

public enum ArenaTier
{
    Bronze, Silver, Gold, Platinum, Diamond, Kage, Legendary
}

public enum TransactionType
{
    MissionReward, BattleReward, ShopPurchase, ShopSale, ClanDonation, AdminGrant, EventReward
}

public enum StatusEffectType
{
    Burn, Paralysis, Bleeding, Poison, Silence, Confusion,
    Stun, Slow, ChakraReduction, Regeneration, Shield,
    AttackBuff, DefenseBuff, AccuracyDebuff, DodgeDebuff
}
