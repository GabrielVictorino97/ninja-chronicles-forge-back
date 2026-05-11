using NarutoPlayers.Domain;

namespace NarutoPlayers.Application.Battle;

public class BattleEngine
{
    private readonly Random _rng = new();

    public Domain.Battle ExecuteAction(Domain.Battle battle, BattleActionType action, string? jutsuName = null)
    {
        if (battle.Status != BattleStatus.Ongoing) return battle;

        var player = battle.Participants.First(p => p.Side == BattleActorSide.Player);
        var enemy = battle.Participants.First(p => p.Side == BattleActorSide.Enemy);

        // Player turn
        if (action == BattleActionType.Flee)
        {
            battle.AddLog("You fled from battle.", BattleActorSide.System);
            battle.End(BattleStatus.Fled);
            return battle;
        }

        if (action == BattleActionType.Defend)
        {
            player.Defending = true;
            battle.AddLog("You assume a defensive stance.", BattleActorSide.Player);
        }
        else if (action == BattleActionType.Item)
        {
            var heal = 80 + _rng.Next(40);
            player.Hp = Math.Min(player.HpMax, player.Hp + heal);
            battle.AddLog($"Used Soldier Pill (+{heal} HP).", BattleActorSide.Player, -heal);
        }
        else if (action == BattleActionType.Basic)
        {
            var dmg = CalculatePhysicalDamage(player, enemy);
            enemy.Hp = Math.Max(0, enemy.Hp - dmg);
            battle.AddLog($"Basic attack on {enemy.Name}.", BattleActorSide.Player, dmg);
        }
        else if (action == BattleActionType.Jutsu)
        {
            const int cost = 25;
            if (player.Chakra < cost)
            {
                battle.AddLog("Not enough chakra!", BattleActorSide.System);
            }
            else
            {
                player.Chakra -= cost;
                var dmg = CalculateNinjutsuDamage(player, enemy);
                enemy.Hp = Math.Max(0, enemy.Hp - dmg);
                var name = jutsuName ?? "Jutsu";
                battle.AddLog($"Used {name} on {enemy.Name}.", BattleActorSide.Player, dmg);
            }
        }

        if (enemy.Hp <= 0)
        {
            battle.AddLog("Victory! Enemy defeated.", BattleActorSide.System);
            battle.End(BattleStatus.Victory);
            return battle;
        }

        // Enemy turn
        var enemyDmg = CalculatePhysicalDamage(enemy, player);
        if (player.Defending) { enemyDmg = (int)(enemyDmg * 0.5); player.Defending = false; }
        player.Hp = Math.Max(0, player.Hp - enemyDmg);
        battle.AddLog($"{enemy.Name} counter-attacked.", BattleActorSide.Enemy, enemyDmg);

        if (player.Hp <= 0)
        {
            battle.AddLog("You were defeated...", BattleActorSide.System);
            battle.End(BattleStatus.Defeat);
            return battle;
        }

        battle.NextTurn();
        return battle;
    }

    private int CalculatePhysicalDamage(BattleParticipant attacker, BattleParticipant defender)
    {
        var baseDmg = attacker.PhysicalAttack + _rng.Next(10, 30);
        if (RollCrit(attacker.CritChance)) baseDmg = (int)(baseDmg * 1.5);
        if (RollDodge(defender.Dodge)) return 0;
        var reduced = Math.Max(1, baseDmg - defender.PhysicalDefense / 2);
        return reduced;
    }

    private int CalculateNinjutsuDamage(BattleParticipant attacker, BattleParticipant defender)
    {
        var baseDmg = attacker.NinjutsuAttack + _rng.Next(20, 50);
        if (RollCrit(attacker.CritChance)) baseDmg = (int)(baseDmg * 1.5);
        if (RollDodge(defender.Dodge - 10)) return 0;
        var reduced = Math.Max(1, baseDmg - defender.SpiritualDefense / 2);
        return reduced;
    }

    private bool RollCrit(int chance) => _rng.Next(100) < Math.Min(chance, 50);
    private bool RollDodge(int dodge) => _rng.Next(100) < Math.Min(dodge, 45);

    public Domain.Battle StartPvE(Character character)
    {
        var battle = Domain.Battle.Create(BattleType.PvE);

        var player = BattleParticipant.Create(battle.Id, BattleActorSide.Player,
            character.Name, character.Avatar,
            character.HpMax, character.ChakraMax, character.Level,
            character.Attributes.Taijutsu * 3 + character.Level,
            character.Attributes.Ninjutsu * 3 + character.Level,
            character.Attributes.Genjutsu * 3 + character.Level,
            character.Attributes.Vitality * 2 + character.Attributes.Taijutsu,
            character.Attributes.Chakra * 2 + character.Attributes.Intelligence,
            character.Attributes.Intelligence * 2 + character.Attributes.Genjutsu,
            character.Attributes.Agility * 2 + character.Attributes.Luck,
            Math.Min(50, 5 + character.Attributes.Luck / 2),
            Math.Min(45, character.Attributes.Agility * 12 / 10),
            50 + character.Attributes.Agility + character.Attributes.Luck / 2);
        player.CharacterId = character.Id;

        var level = Math.Max(1, character.Level - 2 + _rng.Next(5));
        var enemies = new[] { ("Bandit", "🥷"), ("Rogue Ninja", "👤"), ("Missing-nin", "💀"), ("Akatsuki Spy", "☁️") };
        var (enemyName, enemyAvatar) = enemies[_rng.Next(enemies.Length)];

        var enemy = BattleParticipant.Create(battle.Id, BattleActorSide.Enemy,
            enemyName, enemyAvatar,
            80 + level * 20, 50 + level * 10, level,
            8 + level * 2, 5 + level, 3 + level / 2,
            5 + level, 4 + level, 3 + level / 2,
            6 + level, 5, 5, 50 + level);

        battle.Participants.Add(player);
        battle.Participants.Add(enemy);
        battle.AddLog($"Battle started against {enemyName}!", BattleActorSide.System);

        return battle;
    }
}
