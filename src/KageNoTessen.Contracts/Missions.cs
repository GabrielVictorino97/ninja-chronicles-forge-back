namespace KageNoTessen.Contracts.Missions;

public record MissionDto(
    string Id, string Title, string Rank,
    string Description, int EnergyCost,
    int XpReward, int RyousReward, string[] Drops,
    int DurationMinutes, MissionRequirementDto Requirements);

public record MissionRequirementDto(string? Graduation, int? Level);

public record CompleteMissionResponse(int Xp, int Ryous, string[] Drops);

public record MissionHistoryDto(
    string Id, string Title, string Rank,
    DateTime? CompletedAt, int XpReward, int RyousReward);
