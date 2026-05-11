using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Jutsus;
using KageNoTessen.Contracts.Missions;
using KageNoTessen.Domain;

namespace KageNoTessen.Api.Endpoints;

public class GetJutsusEndpoint : EndpointWithoutRequest<IEnumerable<JutsuDto>>
{
    public override void Configure()
    {
        Get("jutsus");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarJutsus")
            .WithSummary("Lista todos os jutsus disponiveis no jogo"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var repo = Resolve<IJutsuRepository>();
        var jutsus = await repo.ListAsync(ct);
        await SendOkAsync(jutsus.OrderBy(j => j.MinLevel).ThenBy(j => j.Name).Select(Map), ct);
    }

    private static JutsuDto Map(Jutsu j) => new(
        j.Id.ToString(), j.Name, j.Type.ToString(), j.Element?.ToString(),
        j.ChakraCost, j.Cooldown, j.BaseDamage, j.Description,
        new RequirementDto(j.MinLevel, null, null));
}

public class GetMyJutsusEndpoint : EndpointWithoutRequest<IEnumerable<CharacterJutsuDto>>
{
    public override void Configure()
    {
        Get("jutsus/me");
        Description(d => d
            .WithName("MeusJutsus")
            .WithSummary("Lista os jutsus aprendidos pelo personagem logado"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var repo = Resolve<ICharacterJutsuRepository>();
        var learned = await repo.GetLearnedAsync(c.Id, ct);
        await SendOkAsync(learned.Select(cj => new CharacterJutsuDto(
            cj.JutsuId.ToString(), cj.Jutsu.Name, cj.Jutsu.Type.ToString(),
            cj.Jutsu.Element?.ToString(), cj.Jutsu.ChakraCost, cj.Jutsu.Cooldown,
            cj.GetBaseDamage(), cj.Jutsu.Description, cj.Equipped, cj.Level)), ct);
    }
}

public class LearnJutsuEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("jutsus/{id:guid}/learn");
        Description(d => d
            .WithName("AprenderJutsu")
            .WithSummary("O personagem logado aprende um jutsu pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetWithDetailsAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var jutsuId = Route<Guid>("id");
        var jutsuRepo = Resolve<IJutsuRepository>();
        var jutsu = await jutsuRepo.GetWithDetailsAsync(jutsuId, ct);
        if (jutsu is null)
        {
            AddError("jutsu", "Jutsu nao encontrado. Use o ID de um jutsu da lista GET /jutsus.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var charJutsuRepo = Resolve<ICharacterJutsuRepository>();
        if (await charJutsuRepo.GetByCharacterAndJutsuAsync(c.Id, jutsuId, ct) is not null)
        {
            AddError("jutsu", "Voce ja aprendeu esse jutsu.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (c.Level < jutsu.MinLevel)
        {
            AddError("level", $"Nivel minimo requerido: {jutsu.MinLevel}. Seu nivel: {c.Level}.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if ((int)c.Graduation < (int)jutsu.MinGraduation)
        {
            AddError("graduation", $"Graduacao minima: {jutsu.MinGraduation}. Sua graduacao: {c.Graduation}.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var cj = CharacterJutsu.Learn(c.Id, jutsuId);
        await charJutsuRepo.AddAsync(cj, ct);
        await SendOkAsync(ct);
    }
}

public class EquipJutsuEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("jutsus/{id:guid}/equip");
        Description(d => d
            .WithName("EquiparJutsu")
            .WithSummary("Equipa um jutsu ja aprendido pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var jutsuId = Route<Guid>("id");
        var repo = Resolve<ICharacterJutsuRepository>();
        var cj = await repo.GetByCharacterAndJutsuAsync(c.Id, jutsuId, ct);
        if (cj is null)
        {
            AddError("jutsu", $"Voce ainda nao aprendeu esse jutsu (ID: {jutsuId}). Use POST /api/jutsus/{jutsuId}/learn primeiro.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (cj.Equipped)
        {
            AddError("jutsu", "Esse jutsu ja esta equipado.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        cj.Equip();
        await repo.UpdateAsync(cj, ct);
        await SendOkAsync(ct);
    }
}

// --- Missions ---
public class GetMissionsEndpoint : EndpointWithoutRequest<IEnumerable<MissionDto>>
{
    public override void Configure()
    {
        Get("missions");
        AllowAnonymous();
        Description(d => d
            .WithName("ListarMissoes")
            .WithSummary("Lista todas as missoes disponiveis"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var repo = Resolve<IMissionRepository>();
        var missions = await repo.ListAsync(ct);
        await SendOkAsync(missions.OrderBy(m => m.Rank).ThenBy(m => m.MinLevel).ThenBy(m => m.Title).Select(m => new MissionDto(
            m.Id.ToString(), m.Title, m.Rank.ToString(), m.Description,
            m.EnergyCost, m.XpReward, m.RyousReward, m.Drops,
            m.DurationMinutes,
            new MissionRequirementDto(m.MinGraduation.ToString(), m.MinLevel))), ct);
    }
}

public class StartMissionEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("missions/{id:guid}/start");
        Description(d => d
            .WithName("IniciarMissao")
            .WithSummary("Inicia uma missao para o personagem logado pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var missionId = Route<Guid>("id");
        var missionRepo = Resolve<IMissionRepository>();
        var mission = await missionRepo.GetByIdAsync(missionId, ct);
        if (mission is null)
        {
            AddError("mission", "Missao nao encontrada. Use o ID de uma missao da lista GET /missions.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var cmRepo = Resolve<ICharacterMissionRepository>();
        if (await cmRepo.GetActiveAsync(c.Id, missionId, ct) is not null)
        {
            AddError("mission", "Voce ja iniciou essa missao. Conclua ela primeiro.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var huntRepo = Resolve<ICharacterHuntRepository>();
        if (await huntRepo.GetActiveAsync(c.Id, ct) is not null)
        {
            AddError("hunt", "Voce esta em uma cacada. Conclua a cacada primeiro antes de iniciar uma missao.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!c.SpendEnergy(mission.EnergyCost))
        {
            AddError("energy", $"Energia insuficiente. Necessario: {mission.EnergyCost}, atual: {c.Energy}.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (c.Level < mission.MinLevel)
        {
            AddError("level", $"Nivel minimo requerido: {mission.MinLevel}. Seu nivel: {c.Level}.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var cm = CharacterMission.Start(c.Id, missionId);
        await cmRepo.AddAsync(cm, ct);
        await charRepo.UpdateAsync(c, ct);
        await SendOkAsync(ct);
    }
}

public class CompleteMissionEndpoint : EndpointWithoutRequest<CompleteMissionResponse>
{
    public override void Configure()
    {
        Post("missions/{id:guid}/complete");
        Description(d => d
            .WithName("ConcluirMissao")
            .WithSummary("Conclui uma missao iniciada e recebe as recompensas"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var missionId = Route<Guid>("id");
        var cmRepo = Resolve<ICharacterMissionRepository>();
        var cm = await cmRepo.GetActiveAsync(c.Id, missionId, ct);
        if (cm is null)
        {
            AddError("mission", "Missao nao iniciada ou ja concluida. Use POST /missions/{id}/start primeiro.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var missionRepo = Resolve<IMissionRepository>();
        var mission = await missionRepo.GetByIdAsync(missionId, ct);
        if (mission is null) { await SendNotFoundAsync(ct); return; }

        if (mission.DurationMinutes > 0 && cm.StartedAt.HasValue)
        {
            var earliest = cm.StartedAt.Value.AddMinutes(mission.DurationMinutes);
            if (DateTime.UtcNow < earliest)
            {
                var remaining = earliest - DateTime.UtcNow;
                AddError("time", $"A missao ainda esta em andamento. Tempo restante: {remaining.Minutes}min {remaining.Seconds}s.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
        }

        cm.Complete();
        await cmRepo.UpdateAsync(cm, ct);

        c.AddXp(mission.XpReward);
        c.AddRyous(mission.RyousReward);
        await charRepo.UpdateAsync(c, ct);

        await SendOkAsync(new CompleteMissionResponse(
            mission.XpReward, mission.RyousReward, mission.Drops), ct);
    }
}

// --- Shopping ---
public class BuyItemEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("shop/buy/{id:guid}");
        Description(d => d
            .WithName("ComprarItem")
            .WithSummary("Compra um item da loja para o personagem logado pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetWithDetailsAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var itemId = Route<Guid>("id");
        var itemRepo = Resolve<IItemRepository>();
        var item = await itemRepo.GetByIdAsync(itemId, ct);
        if (item is null)
        {
            AddError("item", "Item nao encontrado. Use o ID de um item da lista GET /shop/items.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!c.SpendRyous(item.Price))
        {
            AddError("ryous", $"Ryous insuficiente. Preco: {item.Price}, seu saldo: {c.Wallet?.Balance ?? 0}.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var invRepo = Resolve<IInventoryRepository>();
        var existing = await invRepo.GetByCharacterAndItemAsync(c.Id, itemId, ct);
        if (existing is not null)
            existing.AddQuantity(1);
        else
            await invRepo.AddAsync(InventoryItem.Acquire(c.Id, itemId), ct);

        await charRepo.UpdateAsync(c, ct);
        await SendOkAsync(ct);
    }
}

public class GetInventoryEndpoint : EndpointWithoutRequest<IEnumerable<Contracts.Shop.InventoryItemDto>>
{
    public override void Configure()
    {
        Get("inventory");
        Description(d => d
            .WithName("MeuInventario")
            .WithSummary("Lista os itens no inventario do personagem logado"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var repo = Resolve<IInventoryRepository>();
        var items = await repo.GetByCharacterAsync(c.Id, ct);
        await SendOkAsync(items.Select(i => new Contracts.Shop.InventoryItemDto(
            i.ItemId.ToString(), i.Quantity, i.Equipped, i.Slot?.ToString())), ct);
    }
}
