namespace KageNoTessen.Application.Battle;

public class CombatBalanceOptions
{
    public const string Section = "CombatBalance";

    // Attribute multipliers
    public Dictionary<string, double> AttributeMultipliers { get; set; } = new()
    {
        ["Taijutsu"] = 3,
        ["Ninjutsu"] = 3,
        ["Genjutsu"] = 3,
        ["Vitality"] = 2,
        ["Chakra"] = 2,
        ["Agility"] = 2,
        ["Intelligence"] = 1.5,
        ["Luck"] = 1.5
    };

    public int LevelMultiplier { get; set; } = 10;
    public double JutsuDamageMultiplier { get; set; } = 0.5;
    public int DoujutsuBaseBonus { get; set; } = 50;
    public int DoujutsuLevelMultiplier { get; set; } = 2;
    public int ElementBonus { get; set; } = 20;

    // NPC power generation
    public int NpcAttrBaseEasy { get; set; } = 3;
    public int NpcAttrBaseNormal { get; set; } = 5;
    public int NpcAttrBaseHard { get; set; } = 8;
    public int NpcAttrSpread { get; set; } = 5;
    public int NpcJutsuCountEasy { get; set; } = 1;
    public int NpcJutsuCountNormal { get; set; } = 2;
    public int NpcJutsuCountHard { get; set; } = 4;
    public int NpcJutsuPowerEach { get; set; } = 30;

    // Win chance by power ratio
    public Dictionary<string, WinChanceTier> WinChanceTiers { get; set; } = new()
    {
        ["Esmagadora"] = new() { MinRatio = 2.0, WinChance = 95 },
        ["GrandeVantagem"] = new() { MinRatio = 1.5, WinChance = 85 },
        ["Vantagem"] = new() { MinRatio = 1.2, WinChance = 75 },
        ["LevementeFavoravel"] = new() { MinRatio = 1.0, WinChance = 65 },
        ["Equilibrada"] = new() { MinRatio = 0.8, WinChance = 55 },
        ["Desvantagem"] = new() { MinRatio = 0.6, WinChance = 40 },
        ["GrandeDesvantagem"] = new() { MinRatio = 0.4, WinChance = 25 },
        ["QuaseImpossivel"] = new() { MinRatio = 0, WinChance = 10 }
    };

    // Loss penalties
    public double NpcLossMinPercent { get; set; } = 3;
    public double NpcLossMaxPercent { get; set; } = 5;
    public double PvpLossMinPercent { get; set; } = 8;
    public double PvpLossMaxPercent { get; set; } = 10;

    // PvP cooldown in minutes
    public int PvpCooldownMinutes { get; set; } = 10;

    // PvP ryous steal percent range
    public int PvpRyousStealMin { get; set; } = 5;
    public int PvpRyousStealMax { get; set; } = 10;
}

public class WinChanceTier
{
    public double MinRatio { get; set; }
    public int WinChance { get; set; }
}
