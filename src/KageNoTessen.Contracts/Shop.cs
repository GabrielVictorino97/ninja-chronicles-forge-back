namespace KageNoTessen.Contracts.Shop;

public record ItemBonusDto(
    int Attack, int Defense, int Intelligence,
    int Agility, int Vitality, int Chakra, int Luck);

public record ItemDto(
    string Id, string Name, string Type, string Rarity,
    string Description, int Price, string Icon,
    ItemBonusDto Bonuses);

public record InventoryItemDto(
    string ItemId, string Name, string Type, string Rarity,
    string Icon, int Quantity, bool Equipped, string? Slot,
    ItemBonusDto Bonuses);

public record ShopBuyRequest(string ItemId, int Quantity);
public record ShopSellRequest(string ItemId, int Quantity);

public record EquipItemRequest(string? Slot);

public record WalletDto(int Balance);

public record TransactionDto(
    string Id, string Type, int Amount, string Description, DateTime CreatedAt);
