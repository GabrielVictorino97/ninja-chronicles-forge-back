namespace KageNoTessen.Contracts.World;

public record LocationDto(
    string Id, string Name, string Type,
    string GraduationRequired, string[] Enemies,
    string[] MissionIds, string Description);

public record GameEventDto(
    string Id, string Name, string Description, string Type,
    DateTime StartsAt, DateTime EndsAt, double XpMultiplier,
    double DropMultiplier, string[] Rewards, string Status, string Banner);
