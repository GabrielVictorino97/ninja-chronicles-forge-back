using KageNoTessen.Domain;
using Microsoft.Extensions.Options;

namespace KageNoTessen.Application.Battle;

public class CombatPowerCalculator
{
    private readonly CombatBalanceOptions _o;

    public CombatPowerCalculator(IOptions<CombatBalanceOptions> options)
    {
        _o = options.Value;
    }

    public int Calculate(Character c)
    {
        var a = c.Attributes;
        double power = c.Level * _o.LevelMultiplier;

        power += a.Taijutsu * GetAttrMult("Taijutsu");
        power += a.Ninjutsu * GetAttrMult("Ninjutsu");
        power += a.Genjutsu * GetAttrMult("Genjutsu");
        power += a.Vitality * GetAttrMult("Vitality");
        power += a.Chakra * GetAttrMult("Chakra");
        power += a.Agility * GetAttrMult("Agility");
        power += a.Intelligence * GetAttrMult("Intelligence");
        power += a.Luck * GetAttrMult("Luck");

        foreach (var cj in c.CharacterJutsus.Where(cj => cj.Equipped))
        {
            power += cj.GetBaseDamage() * _o.JutsuDamageMultiplier;

            if (cj.Jutsu.Type == JutsuType.Doujutsu)
                power += _o.DoujutsuBaseBonus + c.Level * _o.DoujutsuLevelMultiplier;
        }

        foreach (var inv in c.Inventory.Where(i => i.Equipped))
        {
            var item = inv.Item;
            power += item.AttackBonus * GetAttrMult("Taijutsu");
            power += item.DefenseBonus * GetAttrMult("Vitality");
            power += item.IntelligenceBonus * GetAttrMult("Intelligence");
            power += item.AgilityBonus * GetAttrMult("Agility");
            power += item.VitalityBonus * GetAttrMult("Vitality");
            power += item.ChakraBonus * GetAttrMult("Chakra");
            power += item.LuckBonus * GetAttrMult("Luck");
        }

        power += c.CharacterElements.Count * _o.ElementBonus;

        return (int)Math.Round(power);
    }

    public int CalculateNpcPower(int npcLevel, string difficulty)
    {
        var rng = new Random();
        var attrBase = difficulty switch
        {
            "easy" => _o.NpcAttrBaseEasy,
            "hard" => _o.NpcAttrBaseHard,
            _ => _o.NpcAttrBaseNormal
        };

        double power = npcLevel * _o.LevelMultiplier;
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Taijutsu");
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Ninjutsu");
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Genjutsu");
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Vitality");
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Chakra");
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Agility");
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Intelligence");
        power += rng.Next(attrBase, attrBase + _o.NpcAttrSpread) * GetAttrMult("Luck");

        var jutsuCount = difficulty switch
        {
            "easy" => _o.NpcJutsuCountEasy,
            "hard" => _o.NpcJutsuCountHard,
            _ => _o.NpcJutsuCountNormal
        };
        power += jutsuCount * _o.NpcAttrSpread * _o.JutsuDamageMultiplier * 10;

        return (int)Math.Round(power);
    }

    public (int winChance, string label) GetWinChance(int playerPower, int enemyPower)
    {
        if (enemyPower <= 0) return (_o.WinChanceTiers.First().Value.WinChance, "Esmagadora");

        var ratio = (double)playerPower / enemyPower;

        foreach (var (label, tier) in _o.WinChanceTiers.OrderByDescending(kv => kv.Value.MinRatio))
        {
            if (ratio >= tier.MinRatio)
                return (tier.WinChance, label);
        }

        return (10, "QuaseImpossivel");
    }

    private double GetAttrMult(string name) =>
        _o.AttributeMultipliers.TryGetValue(name, out var m) ? m : 1;
}
