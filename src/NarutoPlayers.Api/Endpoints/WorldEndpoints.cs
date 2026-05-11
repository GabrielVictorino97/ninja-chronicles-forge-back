using FastEndpoints;
using NarutoPlayers.Application.Interfaces;
using NarutoPlayers.Contracts.Characters;

namespace NarutoPlayers.Api.Endpoints;

public class GetVillagesEndpoint : EndpointWithoutRequest<IEnumerable<VillageDto>>
{
    public override void Configure()
    {
        Get("villages");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarVilas")
            .WithSummary("Lista todas as vilas ninja disponiveis"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var repo = Resolve<IVillageRepository>();
        var villages = await repo.ListAsync(ct);
        await SendOkAsync(villages.OrderBy(v => v.Name).Select(v => new VillageDto(
            v.Id.ToString(), v.Name, v.FullName, v.Country,
            v.Description, v.Symbol, v.AccentColor)), ct);
    }
}

public class GetBloodlineClansEndpoint : EndpointWithoutRequest<IEnumerable<BloodlineClanDto>>
{
    public override void Configure()
    {
        Get("bloodline-clans");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarClas")
            .WithSummary("Lista todos os clas de sangue disponiveis"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var repo = Resolve<IBloodlineClanRepository>();
        var clans = await repo.ListAsync(ct);
        await SendOkAsync(clans.OrderBy(c => c.Name).Select(c => new BloodlineClanDto(
            c.Id.ToString(), c.Name, c.Description, c.Bonus, c.Symbol)), ct);
    }
}

public class GetItemsEndpoint : EndpointWithoutRequest<IEnumerable<Contracts.Shop.ItemDto>>
{
    public override void Configure()
    {
        Get("shop/items");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarItens")
            .WithSummary("Lista todos os itens disponiveis na loja"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var repo = Resolve<IItemRepository>();
        var items = await repo.ListAsync(ct);
        await SendOkAsync(items.OrderBy(i => i.Type).ThenBy(i => i.Rarity).Select(i => new Contracts.Shop.ItemDto(
            i.Id.ToString(), i.Name, i.Type.ToString(), i.Rarity.ToString(),
            i.Description, i.Price, i.Icon)), ct);
    }
}
