using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using MediatR;
using KageNoTessen.Application.Characters;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Characters;
namespace KageNoTessen.Api.Endpoints;

public class CreateCharacterEndpoint : Endpoint<CreateCharacterRequest, CharacterDto>
{
    public override void Configure()
    {
        Post("characters");
        Description(d => d
            .WithName("CriarPersonagem")
            .WithSummary("Cria um novo personagem para o usuário logado. Personagens começam sem elemento — use GET /elements e POST /elements/{element}/learn para aprender elementos a partir do nível 20.")
            .Accepts<CreateCharacterRequest>("application/json"));
    }

    public override async Task HandleAsync(CreateCharacterRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        var villageRepo = Resolve<IVillageRepository>();
        var village = await villageRepo.GetByNameAsync(Capitalize(req.VillageId), ct);
        if (village is null)
        {
            AddError(r => r.VillageId, $"Vila '{req.VillageId}' não encontrada. Use: konoha, suna, kiri, kumo, iwa, ame, oto.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var clanRepo = Resolve<IBloodlineClanRepository>();
        var clan = await clanRepo.GetByNameAsync(Capitalize(req.ClanId), ct);
        if (clan is null)
        {
            AddError(r => r.ClanId, $"Clã '{req.ClanId}' não encontrado. Use: uchiha, hyuga, uzumaki, senju, nara, akimichi, etc.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var cmd = new CreateCharacterCommand(userId, req.Name, req.Avatar,
            village.Id, clan.Id);
        var result = await Resolve<IMediator>().Send(cmd, ct);
        await SendCreatedAtAsync<GetCharacterEndpoint>(new { result.Id }, result, cancellation: ct);
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..].ToLower();
}

public class GetMyCharacterEndpoint : EndpointWithoutRequest<CharacterDto>
{
    public override void Configure()
    {
        Get("characters/me");
        Description(d => d
            .WithName("MeuPersonagem")
            .WithSummary("Retorna o personagem do usuário logado"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await Resolve<IMediator>().Send(new GetMyCharacterQuery(userId), ct);
        if (result is null) { await SendNotFoundAsync(ct); return; }
        await SendOkAsync(result, ct);
    }
}

public class GetCharacterEndpoint : EndpointWithoutRequest<CharacterDto>
{
    public override void Configure()
    {
        Get("characters/{id:guid}");
        AllowAnonymous();
        Description(d => d
            .WithName("BuscarPersonagem")
            .WithSummary("Busca um personagem pelo ID"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await Resolve<IMediator>().Send(new GetCharacterQuery(id), ct);
        if (result is null) { await SendNotFoundAsync(ct); return; }
        await SendOkAsync(result, ct);
    }
}

public class UpdateAttributesEndpoint : Endpoint<UpdateAttributesRequest, CharacterDto>
{
    public override void Configure()
    {
        Put("characters/me/attributes");
        Description(d => d
            .WithName("AtualizarAtributos")
            .WithSummary("Distribui pontos de atributos do personagem logado")
            .Accepts<UpdateAttributesRequest>("application/json"));
    }

    public override async Task HandleAsync(UpdateAttributesRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var character = await Resolve<IMediator>().Send(new GetMyCharacterQuery(userId), ct);
        if (character is null) { await SendNotFoundAsync(ct); return; }

        if (req.Attributes is null || req.Attributes.Count == 0)
        {
            AddError("attributes", "Informe ao menos um atributo para distribuir. Ex: { \"taijutsu\": 2, \"ninjutsu\": 1 }");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var validAttrs = new[] { "taijutsu", "ninjutsu", "genjutsu", "intelligence", "vitality", "chakra", "agility", "luck" };
        foreach (var key in req.Attributes.Keys)
        {
            if (!validAttrs.Contains(key.ToLower()))
            {
                AddError("attributes", $"Atributo '{key}' inválido. Use: {string.Join(", ", validAttrs)}.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
        }

        var cmd = new UpdateAttributesCommand(Guid.Parse(character.Id), req.Attributes);
        var result = await Resolve<IMediator>().Send(cmd, ct);
        await SendOkAsync(result, ct);
    }
}
