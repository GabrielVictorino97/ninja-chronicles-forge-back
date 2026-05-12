using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Characters;
using KageNoTessen.Domain;

namespace KageNoTessen.Api.Endpoints;

public class GetElementsEndpoint : EndpointWithoutRequest<IEnumerable<ElementDto>>
{
    public override void Configure()
    {
        Get("elements");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarElementos")
            .WithSummary("Lista todos os elementos disponíveis e o nível necessário para aprender"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var elements = new[]
        {
            (Name: "Katon", Desc: "Elemento Fogo — poderosos ataques ofensivos de chamas."),
            (Name: "Suiton", Desc: "Elemento Água — ataques fluidos e defesa versátil."),
            (Name: "Doton", Desc: "Elemento Terra — defesa sólida e ataques de área."),
            (Name: "Fuuton", Desc: "Elemento Vento — ataques cortantes de alta velocidade."),
            (Name: "Raiton", Desc: "Elemento Raio — ataques rápidos que perfuram defesas."),
        };

        var userIdClaim = HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var learned = new HashSet<ElementAffinity>();
        if (userIdClaim is not null)
        {
            var charRepo = Resolve<ICharacterRepository>();
            var c = await charRepo.GetByUserIdAsync(Guid.Parse(userIdClaim), ct);
            if (c is not null)
            {
                learned = c.CharacterElements.Select(ce => ce.Element).ToHashSet();
            }
        }

        var nextLevel = 20 + learned.Count * 7;
        await SendOkAsync(elements.Select(e => new ElementDto(e.Name, e.Desc, nextLevel,
            learned.Contains(Enum.Parse<ElementAffinity>(e.Name)))), ct);
    }
}

public class LearnElementEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("characters/{characterId:guid}/elements/{element}/learn");
        Description(d => d
            .WithName("AprenderElemento")
            .WithSummary("Aprende um elemento. Progressao: nivel 20 (1º), 27 (2º), 34 (3º), 41 (4º), 48 (5º)."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var elementName = Route<string>("element");
        if (!Enum.TryParse<ElementAffinity>(elementName, true, out var element)
            || element is ElementAffinity.Mokuton or ElementAffinity.Hyoton
                or ElementAffinity.Yoton or ElementAffinity.Jiton
                or ElementAffinity.Bakuton or ElementAffinity.Yin
                or ElementAffinity.Yang or ElementAffinity.YinYang)
        {
            AddError("element", $"Elemento '{elementName}' inválido. Use: Katon, Suiton, Doton, Fuuton, Raiton.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var elemRepo = Resolve<ICharacterElementRepository>();
        if (await elemRepo.GetByCharacterAndElementAsync(c.Id, element, ct) is not null)
        {
            AddError("element", "Você já aprendeu esse elemento.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var learnedCount = await elemRepo.CountByCharacterAsync(c.Id, ct);
        var requiredLevel = 20 + learnedCount * 7;
        if (c.Level < requiredLevel)
        {
            AddError("level", $"Nível insuficiente. Necessário nível {requiredLevel} para aprender o {(learnedCount + 1)}º elemento. Seu nível: {c.Level}.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var ce = CharacterElement.Learn(c.Id, element);
        await elemRepo.AddAsync(ce, ct);
        await SendOkAsync(ct);
    }
}
