namespace KageNoTessen.Contracts.Shop;

public record ItemDto(
    string Id, string Name, string Type, string Rarity,
    string Description, int Price, string Icon);

public record InventoryItemDto(
    string ItemId, int Quantity, bool Equipped, string? Slot);

public record ShopBuyRequest(string ItemId, int Quantity);
public record ShopSellRequest(string ItemId, int Quantity);

public record WalletDto(int Balance);

public record TransactionDto(
    string Id, string Type, int Amount, string Description, DateTime CreatedAt);
