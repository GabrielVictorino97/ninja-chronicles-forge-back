using KageNoTessen.Domain;
using BattleEntity = KageNoTessen.Domain.Battle;

namespace KageNoTessen.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}

public interface ICharacterRepository : IRepository<Character>
{
    Task<Character?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<Character?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Character?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<Character>> GetByUserIdAllAsync(Guid userId, CancellationToken ct = default);
}

public interface IVillageRepository : IRepository<Village>
{
    Task<Village?> GetByNameAsync(string name, CancellationToken ct = default);
}

public interface IBloodlineClanRepository : IRepository<BloodlineClan>
{
    Task<BloodlineClan?> GetByNameAsync(string name, CancellationToken ct = default);
}

public interface IJutsuRepository : IRepository<Jutsu>
{
    Task<Jutsu?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<Jutsu>> GetAvailableForCharacterAsync(Guid characterId, CancellationToken ct = default);
}

public interface ICharacterElementRepository : IRepository<CharacterElement>
{
    Task<List<CharacterElement>> GetByCharacterAsync(Guid characterId, CancellationToken ct = default);
    Task<CharacterElement?> GetByCharacterAndElementAsync(Guid characterId, ElementAffinity element, CancellationToken ct = default);
    Task<int> CountByCharacterAsync(Guid characterId, CancellationToken ct = default);
}

public interface ICharacterJutsuRepository : IRepository<CharacterJutsu>
{
    Task<CharacterJutsu?> GetByCharacterAndJutsuAsync(Guid characterId, Guid jutsuId, CancellationToken ct = default);
    Task<List<CharacterJutsu>> GetEquippedAsync(Guid characterId, CancellationToken ct = default);
    Task<List<CharacterJutsu>> GetLearnedAsync(Guid characterId, CancellationToken ct = default);
    Task<int> CountEquippedAsync(Guid characterId, CancellationToken ct = default);
}

public interface IMissionRepository : IRepository<Mission>
{
    Task<List<Mission>> GetAvailableForCharacterAsync(Guid characterId, CancellationToken ct = default);
}

public interface ICharacterMissionRepository : IRepository<CharacterMission>
{
    Task<CharacterMission?> GetActiveAsync(Guid characterId, Guid missionId, CancellationToken ct = default);
    Task<bool> HasAnyActiveMissionAsync(Guid characterId, CancellationToken ct = default);
    Task<List<CharacterMission>> GetHistoryAsync(Guid characterId, CancellationToken ct = default);
    Task<CharacterMission?> GetLastCompletedAsync(Guid characterId, Guid missionId, CancellationToken ct = default);
}

public interface IBattleRepository : IRepository<BattleEntity>
{
    Task<BattleEntity?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<BattleEntity>> GetHistoryAsync(Guid characterId, int limit = 20, CancellationToken ct = default);
}

public interface IItemRepository : IRepository<Item> { }

public interface IInventoryRepository : IRepository<InventoryItem>
{
    Task<List<InventoryItem>> GetByCharacterAsync(Guid characterId, CancellationToken ct = default);
    Task<InventoryItem?> GetByCharacterAndItemAsync(Guid characterId, Guid itemId, CancellationToken ct = default);
}

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByCharacterIdAsync(Guid characterId, CancellationToken ct = default);
}

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<List<Transaction>> GetByCharacterAsync(Guid characterId, int limit = 50, CancellationToken ct = default);
}

public interface IPlayerClanRepository : IRepository<PlayerClan>
{
    Task<PlayerClan?> GetWithMembersAsync(Guid id, CancellationToken ct = default);
    Task<PlayerClan?> GetByCharacterIdAsync(Guid characterId, CancellationToken ct = default);
    Task<List<PlayerClan>> GetRankingAsync(int limit = 100, CancellationToken ct = default);
}

public interface IWorldLocationRepository : IRepository<WorldLocation> { }

public interface IGameEventRepository : IRepository<GameEvent>
{
    Task<List<GameEvent>> GetActiveAsync(CancellationToken ct = default);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<List<Notification>> GetByUserIdAsync(Guid userId, int limit = 50, CancellationToken ct = default);
}

public interface ICharacterHuntRepository : IRepository<CharacterHunt>
{
    Task<CharacterHunt?> GetActiveAsync(Guid characterId, CancellationToken ct = default);
    Task<CharacterHunt?> GetPendingCompleteAsync(Guid characterId, CancellationToken ct = default);
    Task<int> CountTodayAsync(Guid characterId, CancellationToken ct = default);
}

public interface IAuditLogRepository : IRepository<AuditLog> { }

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> ListAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
