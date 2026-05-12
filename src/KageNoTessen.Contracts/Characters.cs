namespace KageNoTessen.Contracts.Characters;

public record CreateCharacterRequest(
    string Name, string Avatar, string VillageId, string ClanId);

public record UpdateAttributesRequest(Dictionary<string, int> Attributes);

public record CharacterDto(
    string Id, string UserId, string Name, string Avatar,
    string VillageId, string ClanId, string[] Elements,
    string Graduation, int Level, int Xp, int XpToNext,
    int Hp, int HpMax, int Chakra, int ChakraMax,
    int Energy, int EnergyMax, int Ryous, int Power,
    AttributeDto Attributes, int UnspentPoints,
    string[] EquippedJutsus, string[] KnownJutsus, DateTime CreatedAt);

public record AttributeDto(
    int Taijutsu, int Ninjutsu, int Genjutsu, int Intelligence,
    int Vitality, int Chakra, int Agility, int Luck);

public record ProgressionDto(
    int CurrentLevel, int CurrentXp, int XpToNext,
    string CurrentGraduation, string? NextGraduation, int? RequiredLevelForNext);

public record VillageDto(
    string Id, string Name, string FullName, string Country,
    string Description, string Symbol, string AccentColor);

public record BloodlineClanDto(
    string Id, string Name, string Description, string Bonus, string Symbol);

public record ElementDto(
    string Name, string Description, int RequiredLevel, bool Learned);

public record LearnElementRequest(string Element);

public record StartHuntRequest(int DurationMinutes);

public record HuntStatusDto(
    bool Active, int HuntLevel, int DurationMinutes,
    int XpReward, int RyousReward,
    DateTime StartTime, DateTime EndTime, int SecondsRemaining,
    int[] AvailableDurations,
    int TodayHuntsUsed, int TodayHuntsRemaining,
    int TotalAvailableMinutes);

public record HuntRewardDto(int Xp, int Ryous, int DurationMinutes);
