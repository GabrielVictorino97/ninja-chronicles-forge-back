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
        Get("characters/{characterId:guid}/jutsus");
        Description(d => d
            .WithName("MeusJutsus")
            .WithSummary("Lista os jutsus aprendidos pelo personagem"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

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
        Post("characters/{characterId:guid}/jutsus/{id:guid}/learn");
        Description(d => d
            .WithName("AprenderJutsu")
            .WithSummary("O personagem aprende um jutsu pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

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
        Post("characters/{characterId:guid}/jutsus/{id:guid}/equip");
        Description(d => d
            .WithName("EquiparJutsu")
            .WithSummary("Equipa um jutsu ja aprendido pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var jutsuId = Route<Guid>("id");
        var cj = c.CharacterJutsus.FirstOrDefault(cj => cj.JutsuId == jutsuId);
        if (cj is null)
        {
            AddError("jutsu", $"Voce ainda nao aprendeu esse jutsu (ID: {jutsuId}). Use POST /api/characters/{c.Id}/jutsus/{jutsuId}/learn primeiro.");
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
        var repo = Resolve<ICharacterJutsuRepository>();
        await repo.UpdateAsync(cj, ct);
        await SendOkAsync(ct);
    }
}

public class UnequipJutsuEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("characters/{characterId:guid}/jutsus/{id:guid}/unequip");
        Description(d => d
            .WithName("DesequiparJutsu")
            .WithSummary("Desequipa um jutsu que esta equipado pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var jutsuId = Route<Guid>("id");
        var cj = c.CharacterJutsus.FirstOrDefault(cj => cj.JutsuId == jutsuId);
        if (cj is null)
        {
            AddError("jutsu", $"Voce ainda nao aprendeu esse jutsu (ID: {jutsuId}). Use POST /api/characters/{c.Id}/jutsus/{jutsuId}/learn primeiro.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!cj.Equipped)
        {
            AddError("jutsu", "Esse jutsu nao esta equipado.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        cj.Unequip();
        var repo = Resolve<ICharacterJutsuRepository>();
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
        Post("characters/{characterId:guid}/missions/{id:guid}/start");
        Description(d => d
            .WithName("IniciarMissao")
            .WithSummary("Inicia uma missao para o personagem pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

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
        Post("characters/{characterId:guid}/missions/{id:guid}/complete");
        Description(d => d
            .WithName("ConcluirMissao")
            .WithSummary("Conclui uma missao iniciada e recebe as recompensas"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var missionId = Route<Guid>("id");
        var cmRepo = Resolve<ICharacterMissionRepository>();
        var cm = await cmRepo.GetActiveAsync(c.Id, missionId, ct);
        if (cm is null)
        {
            AddError("mission", "Missao nao iniciada ou ja concluida. Use POST /api/characters/{c.Id}/missions/{missionId}/start primeiro.");
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
        Post("characters/{characterId:guid}/shop/buy/{id:guid}");
        Description(d => d
            .WithName("ComprarItem")
            .WithSummary("Compra um item da loja para o personagem pelo ID na URL"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

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
        var existing = c.Inventory.FirstOrDefault(i => i.ItemId == itemId);
        InventoryItem invItem;
        if (existing is not null)
        {
            existing.AddQuantity(1);
            invItem = existing;
        }
        else
        {
            invItem = await invRepo.AddAsync(InventoryItem.Acquire(c.Id, itemId), ct);
        }

        if (item.Equippable && !invItem.Equipped)
        {
            var allItems = c.Inventory.ToList();
            if (existing is null) allItems.Add(invItem);

            EquipSlot? slot = item.Type switch
            {
                ItemType.Weapon => EquipSlot.Weapon,
                ItemType.Armor => EquipSlot.Armor,
                ItemType.Tool => EquipSlot.Tool,
                ItemType.Summon => EquipSlot.Summon,
                ItemType.Accessory => InventoryHelper.ResolveAccessorySlot(allItems),
                _ => null
            };

            if (slot is not null)
            {
                var conflicting = allItems.FirstOrDefault(i => i.Equipped && i.Slot == slot && i.Id != invItem.Id);
                if (conflicting is not null)
                {
                    conflicting.Unequip();
                }
                invItem.Equip(slot.Value);
            }
        }

        await invRepo.UpdateAsync(invItem, ct);
        await charRepo.UpdateAsync(c, ct);
        await SendOkAsync(ct);
    }
}

public class GetInventoryEndpoint : EndpointWithoutRequest<IEnumerable<Contracts.Shop.InventoryItemDto>>
{
    public override void Configure()
    {
        Get("characters/{characterId:guid}/inventory");
        Description(d => d
            .WithName("MeuInventario")
            .WithSummary("Lista os itens no inventario do personagem"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var repo = Resolve<IInventoryRepository>();
        var items = await repo.GetByCharacterAsync(c.Id, ct);
        await SendOkAsync(items.Select(i =>
        {
            var bonus = new Contracts.Shop.ItemBonusDto(
                i.Item.AttackBonus, i.Item.DefenseBonus, i.Item.IntelligenceBonus,
                i.Item.AgilityBonus, i.Item.VitalityBonus, i.Item.ChakraBonus, i.Item.LuckBonus);
            return new Contracts.Shop.InventoryItemDto(
                i.ItemId.ToString(), i.Item.Name, i.Item.Type.ToString(), i.Item.Rarity.ToString(),
                i.Item.Icon, i.Quantity, i.Equipped, i.Slot?.ToString(), bonus);
        }), ct);
    }
}

public class EquipItemEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("characters/{characterId:guid}/inventory/{itemId:guid}/equip");
        Description(d => d
            .WithName("EquiparItem")
            .WithSummary("Equipa um item do inventario. Substitui automaticamente o item do mesmo tipo se ja estiver equipado."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var itemId = Route<Guid>("itemId");
        var allItems = c.Inventory.ToList();

        var inv = allItems.FirstOrDefault(i => i.ItemId == itemId);
        if (inv is null)
        {
            AddError("item", "Item nao encontrado no inventario.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!inv.Item.Equippable)
        {
            AddError("item", "Esse item nao pode ser equipado.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (inv.Equipped)
        {
            AddError("item", "Esse item ja esta equipado.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var slot = inv.Item.Type switch
        {
            ItemType.Weapon => EquipSlot.Weapon,
            ItemType.Armor => EquipSlot.Armor,
            ItemType.Tool => EquipSlot.Tool,
            ItemType.Summon => EquipSlot.Summon,
            ItemType.Accessory => InventoryHelper.ResolveAccessorySlot(allItems),
            _ => (EquipSlot?)null
        };

        if (slot is null)
        {
            AddError("item", $"Itens do tipo {inv.Item.Type} nao podem ser equipados.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var conflicting = allItems.FirstOrDefault(i => i.Equipped && i.Slot == slot && i.Id != inv.Id);
        if (conflicting is not null)
        {
            conflicting.Unequip();
        }

        inv.Equip(slot.Value);
        var invRepo = Resolve<IInventoryRepository>();
        await invRepo.UpdateAsync(inv, ct);
        await SendOkAsync(ct);
    }
}

internal static class InventoryHelper
{
    public static EquipSlot ResolveAccessorySlot(List<InventoryItem> allItems)
    {
        var used = allItems.Where(i => i.Equipped && (i.Slot == EquipSlot.Accessory1 || i.Slot == EquipSlot.Accessory2))
            .Select(i => i.Slot).ToHashSet();
        if (!used.Contains(EquipSlot.Accessory1)) return EquipSlot.Accessory1;
        if (!used.Contains(EquipSlot.Accessory2)) return EquipSlot.Accessory2;
        return EquipSlot.Accessory1;
    }
}

public class UnequipItemEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("characters/{characterId:guid}/inventory/{itemId:guid}/unequip");
        Description(d => d
            .WithName("DesequiparItem")
            .WithSummary("Desequipa um item do inventario"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var itemId = Route<Guid>("itemId");
        var inv = c.Inventory.FirstOrDefault(i => i.ItemId == itemId);
        if (inv is null)
        {
            AddError("item", "Item nao encontrado no inventario.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!inv.Equipped)
        {
            AddError("item", "Esse item nao esta equipado.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        inv.Unequip();
        var invRepo = Resolve<IInventoryRepository>();
        await invRepo.UpdateAsync(inv, ct);
        await SendOkAsync(ct);
    }
}
