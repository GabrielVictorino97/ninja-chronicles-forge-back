using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Admin;
using KageNoTessen.Domain;

namespace KageNoTessen.Api.Endpoints;

public abstract class AdminEndpoint
{
    internal static async Task<bool> RequireAdmin(HttpContext httpContext, CancellationToken ct)
    {
        var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        if (role is not ("Admin" or "SuperAdmin"))
        {
            httpContext.Response.StatusCode = 403;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync("{\"detail\":\"Acesso restrito a administradores.\"}", ct);
            return false;
        }
        return true;
    }
}

public class GetAdminDashboardEndpoint : EndpointWithoutRequest<AdminDashboardDto>
{
    public override void Configure()
    {
        Get("admin/dashboard");
        Description(d => d.WithName("PainelAdmin").WithSummary("Dashboard administrativo"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var users = Resolve<IUserRepository>();
        var characters = Resolve<ICharacterRepository>();
        var battles = Resolve<IBattleRepository>();
        var clans = Resolve<IPlayerClanRepository>();

        var today = DateTime.UtcNow.Date;
        var allUsers = await users.ListAsync(ct);
        var allChars = await characters.ListAsync(ct);
        var allBattles = await battles.ListAsync(ct);
        var allClans = await clans.ListAsync(ct);

        await SendOkAsync(new AdminDashboardDto(
            allUsers.Count,
            allUsers.Count(u => u.Status == UserStatus.Active),
            allChars.Count,
            allBattles.Count(b => b.CreatedAt >= today),
            0, // missions today
            allClans.Count,
            0, 0, 0, 0, // events, reports, banned, transactions
            Array.Empty<ChartPoint>(),
            Array.Empty<ChartPoint>(),
            Array.Empty<RankChartPoint>(),
            Array.Empty<DistributionPoint>(),
            Array.Empty<DistributionPoint>()), ct);
    }
}

public class GetAdminUsersEndpoint : EndpointWithoutRequest<IEnumerable<AdminUserDto>>
{
    public override void Configure()
    {
        Get("admin/users");
        Description(d => d.WithName("ListarUsuariosAdmin").WithSummary("Lista todos os usuarios"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var repo = Resolve<IUserRepository>();
        var users = await repo.ListAsync(ct);
        await SendOkAsync(users.OrderByDescending(u => u.CreatedAt).Select(u =>
            new AdminUserDto(u.Id.ToString(), u.Name, u.Email, u.Role.ToString(),
                u.Status.ToString(), u.CreatedAt,
                u.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "-",
                u.LastLoginIp ?? "-")), ct);
    }
}

public class GetAdminCharactersEndpoint : EndpointWithoutRequest<IEnumerable<Contracts.Characters.CharacterDto>>
{
    public override void Configure()
    {
        Get("admin/characters");
        Description(d => d.WithName("ListarPersonagensAdmin").WithSummary("Lista todos os personagens"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var repo = Resolve<ICharacterRepository>();
        var chars = await repo.ListAsync(ct);
        await SendOkAsync(chars.OrderByDescending(c => c.Power).Select(c =>
            new Contracts.Characters.CharacterDto(
                c.Id.ToString(), c.UserId.ToString(), c.Name, c.Avatar,
                c.Village.Name.ToLower(), c.Clan.Name.ToLower(),
                c.CharacterElements.Select(ce => ce.Element.ToString()).ToArray(),
                c.Graduation.ToString(), c.Level, c.Xp, c.XpToNext,
                c.Hp, c.HpMax, c.Chakra, c.ChakraMax,
                c.Energy, c.EnergyMax, c.Ryous, c.Power,
                new Contracts.Characters.AttributeDto(
                    c.Attributes.Taijutsu, c.Attributes.Ninjutsu, c.Attributes.Genjutsu,
                    c.Attributes.Intelligence, c.Attributes.Vitality,
                    c.Attributes.Chakra, c.Attributes.Agility, c.Attributes.Luck),
                c.UnspentPoints,
                Array.Empty<string>(), Array.Empty<string>(),
                c.CreatedAt)), ct);
    }
}

public class GetAdminBattlesEndpoint : EndpointWithoutRequest<IEnumerable<object>>
{
    public override void Configure()
    {
        Get("admin/battles");
        Description(d => d.WithName("ListarBatalhasAdmin").WithSummary("Lista batalhas recentes"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var repo = Resolve<IBattleRepository>();
        var battles = await repo.ListAsync(ct);
        var recent = battles.OrderByDescending(b => b.CreatedAt).Take(50);
        await SendOkAsync(recent.Select(b =>
        {
            var players = b.Participants.ToList();
            return new
            {
                id = b.Id.ToString(),
                player1 = players.FirstOrDefault(p => p.Side == Domain.BattleActorSide.Player)?.Name ?? "?",
                player2 = players.FirstOrDefault(p => p.Side == Domain.BattleActorSide.Enemy)?.Name ?? "?",
                type = b.Type.ToString(),
                winner = b.Status == Domain.BattleStatus.Victory ? players.FirstOrDefault(p => p.Side == Domain.BattleActorSide.Player)?.Name
                    : b.Status == Domain.BattleStatus.Defeat ? players.FirstOrDefault(p => p.Side == Domain.BattleActorSide.Enemy)?.Name
                    : "-",
                duration = "N/A",
                date = b.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                status = b.Status == Domain.BattleStatus.Victory ? "completed" : b.Status == Domain.BattleStatus.Defeat ? "completed" : b.Status == Domain.BattleStatus.Fled ? "abandoned" : "completed",
                turns = b.Logs.Select(l => new { n = l.Turn, actor = l.Actor.ToString().ToLower(), action = l.Message, damage = l.Damage ?? 0 }).ToArray(),
                rewards = Array.Empty<string>()
            };
        }), ct);
    }
}

public class GetAdminRankingEndpoint : EndpointWithoutRequest<IEnumerable<Contracts.Clans.RankingPlayerDto>>
{
    public override void Configure()
    {
        Get("admin/rankings");
        Description(d => d.WithName("RankingAdmin").WithSummary("Ranking administravel"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var repo = Resolve<ICharacterRepository>();
        var chars = await repo.ListAsync(ct);
        var ranking = chars.Where(c => c.Active).OrderByDescending(c => c.Power).Take(100).ToList();
        var result = new List<Contracts.Clans.RankingPlayerDto>();
        for (int i = 0; i < ranking.Count; i++)
        {
            var c = ranking[i];
            result.Add(new Contracts.Clans.RankingPlayerDto(
                i + 1, c.Name, c.Village?.Name ?? "", c.Clan?.Name ?? "",
                c.Level, c.Graduation.ToString(), c.Power, 0));
        }
        await SendOkAsync(result, ct);
    }
}

public class GetAdminVillagesEndpoint : EndpointWithoutRequest<IEnumerable<Contracts.Characters.VillageDto>>
{
    public override void Configure()
    {
        Get("admin/villages");
        Description(d => d.WithName("ListarVilasAdmin").WithSummary("Lista todas as vilas"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var repo = Resolve<IVillageRepository>();
        var villages = await repo.ListAsync(ct);
        await SendOkAsync(villages.Select(v =>
            new Contracts.Characters.VillageDto(v.Name.ToLower(), v.Name, v.FullName,
                v.Country, v.Description, v.Symbol, v.AccentColor)), ct);
    }
}

public class AdminBanUserEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("admin/users/{id:guid}/ban");
        Description(d => d.WithName("BanirUsuario").WithSummary("Bane um usuario pelo ID"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var id = Route<Guid>("id");
        var repo = Resolve<IUserRepository>();
        var user = await repo.GetByIdAsync(id, ct);
        if (user is null) { await SendNotFoundAsync(ct); return; }

        user.Ban();
        await repo.UpdateAsync(user, ct);
        await SendOkAsync(new { id = id.ToString(), status = "banned" }, ct);
    }
}

public class AdminUnbanUserEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("admin/users/{id:guid}/unban");
        Description(d => d.WithName("DesbanirUsuario").WithSummary("Desbane um usuario pelo ID"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var id = Route<Guid>("id");
        var repo = Resolve<IUserRepository>();
        var user = await repo.GetByIdAsync(id, ct);
        if (user is null) { await SendNotFoundAsync(ct); return; }

        user.Unban();
        await repo.UpdateAsync(user, ct);
        await SendOkAsync(new { id = id.ToString(), status = "active" }, ct);
    }
}

public class AdminSetRoleEndpoint : Endpoint<AdminSetRoleRequest>
{
    public override void Configure()
    {
        Put("admin/users/{id:guid}/role");
        Description(d => d.WithName("AlterarCargo").WithSummary("Altera o cargo de um usuario").Accepts<AdminSetRoleRequest>("application/json"));
    }

    public override async Task HandleAsync(AdminSetRoleRequest req, CancellationToken ct)
    {
        if (!await AdminEndpoint.RequireAdmin(HttpContext, ct)) return;

        var id = Route<Guid>("id");
        var repo = Resolve<IUserRepository>();
        var user = await repo.GetByIdAsync(id, ct);
        if (user is null) { await SendNotFoundAsync(ct); return; }

        if (!Enum.TryParse<UserRole>(req.Role, true, out var role))
        {
            AddError("role", "Cargo invalido. Use: Player, Moderator, Admin, SuperAdmin.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        user.SetRole(role);
        await repo.UpdateAsync(user, ct);
        await SendOkAsync(new { id = id.ToString(), role = role.ToString() }, ct);
    }
}

public record AdminSetRoleRequest(string Role);
