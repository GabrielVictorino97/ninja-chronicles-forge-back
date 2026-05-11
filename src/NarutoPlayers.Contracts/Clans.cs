namespace NarutoPlayers.Contracts.Clans;

public record PlayerClanDto(
    string Id, string Name, string Tag, int Level,
    int Xp, int XpToNext, ClanMemberDto[] Members,
    int Ranking, ClanWallPostDto[] Wall);

public record ClanMemberDto(
    string CharacterId, string Name, int Level, string Role, int Donations);

public record ClanWallPostDto(
    string Id, string Author, string Message, string Date);

public record CreateClanRequest(string Name, string Tag);
