namespace KageNoTessen.Contracts.Battle;

public record BattleDto(
    string Id, BattleActorDto Player, BattleActorDto Enemy,
    int Turn, bool IsPlayerTurn, BattleLogEntryDto[] Log,
    string Status, StatusEffectDto[] StatusEffects);

public record BattleActorDto(
    string Id, string Name, string Avatar,
    int Hp, int HpMax, int Chakra, int ChakraMax, int Level);

public record BattleLogEntryDto(
    string Id, int Turn, string Actor, string Message, int? Damage);

public record StatusEffectDto(string Actor, string Name, int Turns);

public record BattleAction(string Action, string? JutsuName = null);

public record BattleActionResult(BattleDto Battle, string[]? Rewards = null);

public record NpcBattleRequest(string Difficulty);

public record PvpBattleRequest(string TargetName);

public record BattleResultDto(
    string Result, string EnemyName, int EnemyLevel,
    string Difficulty, int XpReward, int RyousReward,
    int PlayerLevel, string PlayerGraduation,
    int PlayerPower, int EnemyPower, string PowerComparison);
