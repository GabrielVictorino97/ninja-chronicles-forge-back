namespace NarutoPlayers.Contracts.Clans;

public record RankingPlayerDto(
    int Position, string Name, string Village, string Clan,
    int Level, string Graduation, int Power, int Wins);
