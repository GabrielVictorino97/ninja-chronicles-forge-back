using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Clans;
using KageNoTessen.Domain;

namespace KageNoTessen.Api.Endpoints;

public class GetMyClanEndpoint : EndpointWithoutRequest<PlayerClanDto?>
{
    public override void Configure()
    {
        Get("clans/me");
        Description(d => d
            .WithName("MeuCla")
            .WithSummary("Retorna o cla do personagem logado, ou null se nao estiver em um"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var clanRepo = Resolve<IPlayerClanRepository>();
        var clan = await clanRepo.GetByCharacterIdAsync(c.Id, ct);
        if (clan is null) { await SendOkAsync((PlayerClanDto?)null, ct); return; }

        await SendOkAsync(Map(clan), ct);
    }

    private static PlayerClanDto Map(PlayerClan clan) => new(
        clan.Id.ToString(), clan.Name, clan.Tag, clan.Level,
        clan.Xp, clan.XpToNext,
        clan.Members.OrderBy(m => m.Role).Select(m =>
            new ClanMemberDto(m.CharacterId.ToString(), m.Name, m.Level, m.Role.ToString(), m.Donations)).ToArray(),
        clan.Ranking,
        clan.Wall.OrderByDescending(w => w.Date).Take(20).Select(w =>
            new ClanWallPostDto(w.Id.ToString(), w.Author, w.Message, w.Date.ToString("yyyy-MM-dd"))).ToArray());
}

public class CreateClanEndpoint : Endpoint<CreateClanRequest>
{
    public override void Configure()
    {
        Post("clans");
        Description(d => d
            .WithName("CriarCla")
            .WithSummary("Cria um novo cla e adiciona o personagem como lider")
            .Accepts<CreateClanRequest>("application/json"));
    }

    public override async Task HandleAsync(CreateClanRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { AddError("character", "Personagem nao encontrado."); await SendErrorsAsync(cancellation: ct); return; }

        if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length < 3)
        {
            AddError("name", "Nome do cla deve ter pelo menos 3 caracteres.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var clanRepo = Resolve<IPlayerClanRepository>();
        if (await clanRepo.GetByCharacterIdAsync(c.Id, ct) is not null)
        {
            AddError("clan", "Voce ja pertence a um cla.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var clan = PlayerClan.Create(req.Name, req.Tag ?? req.Name[..Math.Min(4, req.Name.Length)].ToUpper(), c.Id, c.Name);
        await clanRepo.AddAsync(clan, ct);
        await SendOkAsync(ct);
    }
}

public class JoinClanEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("clans/{id:guid}/join");
        Description(d => d
            .WithName("EntrarCla")
            .WithSummary("Entra em um cla pelo ID"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { AddError("character", "Personagem nao encontrado."); await SendErrorsAsync(cancellation: ct); return; }

        var clanId = Route<Guid>("id");
        var clanRepo = Resolve<IPlayerClanRepository>();
        var clan = await clanRepo.GetByIdAsync(clanId, ct);
        if (clan is null) { AddError("clan", "Cla nao encontrado."); await SendErrorsAsync(cancellation: ct); return; }

        if (await clanRepo.GetByCharacterIdAsync(c.Id, ct) is not null)
        {
            AddError("clan", "Voce ja pertence a um cla.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        clan.Members.Add(PlayerClanMember.Create(clan.Id, c.Id, c.Name, ClanRole.Recruit));
        await clanRepo.UpdateAsync(clan, ct);
        await SendOkAsync(ct);
    }
}

public class DonateClanEndpoint : Endpoint<DonateClanRequest>
{
    public override void Configure()
    {
        Post("clans/donate");
        Description(d => d
            .WithName("DoarCla")
            .WithSummary("Doa ryous para o cla")
            .Accepts<DonateClanRequest>("application/json"));
    }

    public override async Task HandleAsync(DonateClanRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { AddError("character", "Personagem nao encontrado."); await SendErrorsAsync(cancellation: ct); return; }

        var clanRepo = Resolve<IPlayerClanRepository>();
        var clan = await clanRepo.GetByCharacterIdAsync(c.Id, ct);
        if (clan is null) { AddError("clan", "Voce nao pertence a um cla."); await SendErrorsAsync(cancellation: ct); return; }

        if (req.Amount <= 0)
        {
            AddError("amount", "Valor da doacao deve ser positivo.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!c.SpendRyous(req.Amount))
        {
            AddError("ryous", $"Ryous insuficiente. Saldo: {c.Ryous}.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var member = clan.Members.First(m => m.CharacterId == c.Id);
        member.Donate(req.Amount);
        clan.AddXp(req.Amount / 10);
        await clanRepo.UpdateAsync(clan, ct);
        await charRepo.UpdateAsync(c, ct);
        await SendOkAsync(new { ok = true, amount = req.Amount }, ct);
    }
}

// DTO para doação
public record DonateClanRequest(int Amount);
