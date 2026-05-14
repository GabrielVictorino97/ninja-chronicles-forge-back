using FastEndpoints;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Characters;
using KageNoTessen.Contracts.World;
using KageNoTessen.Domain;

namespace KageNoTessen.Api.Endpoints;

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
            v.Name.ToLower(), v.Name, v.FullName, v.Country,
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
            c.Name.ToLower(), c.Name, c.Description, c.Bonus, c.Symbol)), ct);
    }
}

// --- World Locations ---

public class GetWorldLocationsEndpoint : EndpointWithoutRequest<IEnumerable<LocationDto>>
{
    public override void Configure()
    {
        Get("world/locations");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarLocais")
            .WithSummary("Lista todos os locais do mundo ninja"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var repo = Resolve<IWorldLocationRepository>();
        var locations = await repo.ListAsync(ct);
        await SendOkAsync(locations.OrderBy(l => l.GraduationRequired).ThenBy(l => l.Name).Select(l =>
            new LocationDto(l.Id.ToString(), l.Name, l.Type.ToString(),
                l.GraduationRequired.ToString(), l.Enemies, Array.Empty<string>(), l.Description)), ct);
    }
}

public class TravelEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("world/travel/{locationId:guid}");
        Description(d => d
            .WithName("Viajar")
            .WithSummary("Viaja para um local do mundo (decorativo)"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var locationId = Route<Guid>("locationId");
        var repo = Resolve<IWorldLocationRepository>();
        var location = await repo.GetByIdAsync(locationId, ct);
        if (location is null) { AddError("location", "Local nao encontrado."); await SendErrorsAsync(cancellation: ct); return; }

        await SendOkAsync(new { ok = true, locationId = locationId.ToString() }, ct);
    }
}

// --- Game Events ---

public class GetGameEventsEndpoint : EndpointWithoutRequest<IEnumerable<GameEventDto>>
{
    public override void Configure()
    {
        Get("events");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarEventos")
            .WithSummary("Lista os eventos ativos no jogo"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var repo = Resolve<IGameEventRepository>();
        var events = await repo.GetActiveAsync(ct);
        await SendOkAsync(events.Select(e => new GameEventDto(
            e.Id.ToString(), e.Name, e.Description, e.Type.ToString(),
            e.StartsAt, e.EndsAt, e.XpMultiplier, e.DropMultiplier,
            e.Rewards, e.Status.ToString(), e.Banner)), ct);
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
        await SendOkAsync(items.OrderBy(i => i.Type).ThenBy(i => i.Rarity).Select(i =>
        {
            var bonus = new Contracts.Shop.ItemBonusDto(
                i.AttackBonus, i.DefenseBonus, i.IntelligenceBonus,
                i.AgilityBonus, i.VitalityBonus, i.ChakraBonus, i.LuckBonus);
            return new Contracts.Shop.ItemDto(
                i.Id.ToString(), i.Name, i.Type.ToString(), i.Rarity.ToString(),
                i.Description, i.Price, i.Icon, bonus);
        }), ct);
    }
}
