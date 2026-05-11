namespace NarutoPlayers.Contracts.Jutsus;

public record JutsuDto(
    string Id, string Name, string Type,
    string? Element, int ChakraCost, int Cooldown,
    int BaseDamage, string Description,
    RequirementDto Requirements);

public record RequirementDto(int Level, string? Attribute, int? Value);

public record CharacterJutsuDto(
    string Id, string Name, string Type,
    string? Element, int ChakraCost, int Cooldown,
    int BaseDamage, string Description,
    bool Equipped, int LearnedLevel);

public record LearnJutsuRequest(string JutsuId);
