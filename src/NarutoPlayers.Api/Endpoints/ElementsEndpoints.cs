using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using NarutoPlayers.Application.Interfaces;
using NarutoPlayers.Contracts.Characters;
using NarutoPlayers.Domain;

namespace NarutoPlayers.Api.Endpoints;

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
        var learnedCount = 0;
        if (userIdClaim is not null)
        {
            var charRepo = Resolve<ICharacterRepository>();
            var c = await charRepo.GetByUserIdAsync(Guid.Parse(userIdClaim), ct);
            if (c is not null)
            {
                var elemRepo = Resolve<ICharacterElementRepository>();
                learnedCount = await elemRepo.CountByCharacterAsync(c.Id, ct);
            }
        }

        var nextLevel = 20 + learnedCount * 10;
        await SendOkAsync(elements.Select(e => new ElementDto(e.Name, e.Desc, nextLevel)), ct);
    }
}

public class LearnElementEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("elements/{element}/learn");
        Description(d => d
            .WithName("AprenderElemento")
            .WithSummary("Aprende um elemento. Requer nível 20 para o 1º elemento, 30 para o 2º, 40 para o 3º, etc."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

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
        var requiredLevel = 20 + learnedCount * 10;
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
