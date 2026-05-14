using FastEndpoints;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Clans;
using KageNoTessen.Domain;

namespace KageNoTessen.Api.Endpoints;

public class GetRankingEndpoint : EndpointWithoutRequest<IEnumerable<RankingPlayerDto>>
{
    public override void Configure()
    {
        Get("ranking");
        AllowAnonymous();
        Description(d => d
            .WithName("Ranking")
            .WithSummary("Retorna o ranking global dos jogadores por poder de combate"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var charRepo = Resolve<ICharacterRepository>();
        var characters = await charRepo.ListAsync(ct);
        var active = characters
            .Where(c => c.Active)
            .OrderByDescending(c => c.Power)
            .ThenByDescending(c => c.Level)
            .Take(100)
            .ToList();

        var result = new List<RankingPlayerDto>();
        for (int i = 0; i < active.Count; i++)
        {
            var c = active[i];
            result.Add(new RankingPlayerDto(
                i + 1, c.Name,
                c.Village?.Name ?? "Desconhecida",
                c.Clan?.Name ?? "Sem cla",
                c.Level, c.Graduation.ToString(), c.Power, 0));
        }

        await SendOkAsync(result, ct);
    }
}
