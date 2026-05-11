namespace KageNoTessen.Contracts.Admin;

public record AdminDashboardDto(
    int TotalUsers, int ActiveUsers, int CharactersCreated,
    int BattlesToday, int MissionsToday, int ActiveClans,
    int ActiveEvents, int PendingReports, int BannedUsers, int Transactions,
    ChartPoint[] NewUsersByDay, ChartPoint[] BattlesByDay,
    RankChartPoint[] MissionsByRank, DistributionPoint[] VillagesDistribution,
    DistributionPoint[] ClansDistribution);

public record ChartPoint(string Date, int Value);
public record RankChartPoint(string Rank, int Value);
public record DistributionPoint(string Name, int Value);

public record AdminUserDto(
    string Id, string Name, string Email, string Role,
    string Status, DateTime CreatedAt, string LastLogin, string Ip);
