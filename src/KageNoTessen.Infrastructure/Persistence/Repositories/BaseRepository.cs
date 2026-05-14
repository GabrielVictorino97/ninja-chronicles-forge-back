using Microsoft.EntityFrameworkCore;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Domain;

namespace KageNoTessen.Infrastructure.Persistence.Repositories;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    public BaseRepository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<List<T>> ListAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        var entry = await Set.AddAsync(entity, ct);
        await Db.SaveChangesAsync(ct);
        return entry.Entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        Set.Update(entity);
        await Db.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        Set.Remove(entity);
        await Db.SaveChangesAsync(ct);
    }
}

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext db) : base(db) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
        => await Set.Where(rt => rt.UserId == userId && !rt.Revoked)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.Revoked, true), ct);
}

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await Set.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);
}

public class CharacterRepository : BaseRepository<Character>, ICharacterRepository
{
    public CharacterRepository(AppDbContext db) : base(db) { }

    public override async Task<List<Character>> ListAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(c => c.Village)
            .Include(c => c.Clan)
            .Include(c => c.Attributes)
            .Include(c => c.CharacterElements)
            .ToListAsync(ct);

    public async Task<Character?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Include(c => c.Attributes)
            .Include(c => c.Village)
            .Include(c => c.Clan)
            .Include(c => c.Wallet)
            .Include(c => c.CharacterElements)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task LoadReferencesAsync(Character character, CancellationToken ct = default)
    {
        await Db.Entry(character).Reference(c => c.Village).LoadAsync(ct);
        await Db.Entry(character).Reference(c => c.Clan).LoadAsync(ct);
        await Db.Entry(character).Reference(c => c.Attributes).LoadAsync(ct);
        await Db.Entry(character).Collection(c => c.CharacterElements).LoadAsync(ct);
    }

    public async Task<Character?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(c => c.Attributes)
            .Include(c => c.Village)
            .Include(c => c.Clan)
            .Include(c => c.Wallet)
            .Include(c => c.CharacterElements)
            .Include(c => c.CharacterJutsus).ThenInclude(cj => cj.Jutsu)
            .Include(c => c.Inventory).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Active, ct);

    public async Task<Character?> GetByNameAsync(string name, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(c => c.Attributes)
            .Include(c => c.Village)
            .Include(c => c.Clan)
            .Include(c => c.Wallet)
            .Include(c => c.CharacterElements)
            .Include(c => c.CharacterJutsus).ThenInclude(cj => cj.Jutsu)
            .Include(c => c.Inventory).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(c => c.Name == name && c.Active, ct);

    public async Task<List<Character>> GetByUserIdAllAsync(Guid userId, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(c => c.UserId == userId).ToListAsync(ct);

    public async Task<List<Character>> ListAllWithVillageAndClanAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().Include(c => c.Village).Include(c => c.Clan).ToListAsync(ct);
}

public class JutsuRepository : BaseRepository<Jutsu>, IJutsuRepository
{
    public JutsuRepository(AppDbContext db) : base(db) { }

    public Task<Jutsu?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => Set.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<List<Jutsu>> GetAvailableForCharacterAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(j => j.Active).ToListAsync(ct);
}

public class CharacterElementRepository : BaseRepository<CharacterElement>, ICharacterElementRepository
{
    public CharacterElementRepository(AppDbContext db) : base(db) { }

    public async Task<List<CharacterElement>> GetByCharacterAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(ce => ce.CharacterId == characterId).ToListAsync(ct);

    public async Task<CharacterElement?> GetByCharacterAndElementAsync(Guid characterId, ElementAffinity element, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(ce => ce.CharacterId == characterId && ce.Element == element, ct);

    public Task<int> CountByCharacterAsync(Guid characterId, CancellationToken ct = default)
        => Set.CountAsync(ce => ce.CharacterId == characterId, ct);
}

public class CharacterJutsuRepository : BaseRepository<CharacterJutsu>, ICharacterJutsuRepository
{
    public CharacterJutsuRepository(AppDbContext db) : base(db) { }

    public async Task<CharacterJutsu?> GetByCharacterAndJutsuAsync(Guid characterId, Guid jutsuId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(cj => cj.CharacterId == characterId && cj.JutsuId == jutsuId, ct);

    public async Task<List<CharacterJutsu>> GetEquippedAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(cj => cj.Jutsu).Where(cj => cj.CharacterId == characterId && cj.Equipped).ToListAsync(ct);

    public async Task<List<CharacterJutsu>> GetLearnedAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(cj => cj.Jutsu).Where(cj => cj.CharacterId == characterId).ToListAsync(ct);

    public Task<int> CountEquippedAsync(Guid characterId, CancellationToken ct = default)
        => Set.CountAsync(cj => cj.CharacterId == characterId && cj.Equipped, ct);
}

public class CharacterMissionRepository : BaseRepository<CharacterMission>, ICharacterMissionRepository
{
    public CharacterMissionRepository(AppDbContext db) : base(db) { }

    public async Task<CharacterMission?> GetActiveAsync(Guid characterId, Guid missionId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(cm =>
            cm.CharacterId == characterId && cm.MissionId == missionId && !cm.Completed, ct);

    public Task<bool> HasAnyActiveMissionAsync(Guid characterId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(cm => cm.CharacterId == characterId && !cm.Completed, ct);

    public async Task<List<CharacterMission>> GetHistoryAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(cm => cm.Mission)
            .Where(cm => cm.CharacterId == characterId && cm.Completed)
            .OrderByDescending(cm => cm.CompletedAt)
            .Take(50).ToListAsync(ct);

    public async Task<CharacterMission?> GetLastCompletedAsync(Guid characterId, Guid missionId, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(cm => cm.CharacterId == characterId && cm.MissionId == missionId && cm.Completed)
            .OrderByDescending(cm => cm.CompletedAt)
            .FirstOrDefaultAsync(ct);
}

public class BattleRepository : BaseRepository<Battle>, IBattleRepository
{
    public BattleRepository(AppDbContext db) : base(db) { }

    public async Task<Battle?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(b => b.Participants).Include(b => b.Logs)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<List<Battle>> GetHistoryAsync(Guid characterId, int limit = 20, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(b => b.Participants)
            .Where(b => b.Participants.Any(p => p.CharacterId == characterId))
            .OrderByDescending(b => b.CreatedAt)
            .Take(limit).ToListAsync(ct);
}

public class InventoryRepository : BaseRepository<InventoryItem>, IInventoryRepository
{
    public InventoryRepository(AppDbContext db) : base(db) { }

    public async Task<List<InventoryItem>> GetByCharacterAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(i => i.Item)
            .Where(i => i.CharacterId == characterId && i.Quantity > 0)
            .ToListAsync(ct);

    public async Task<InventoryItem?> GetByCharacterAndItemAsync(Guid characterId, Guid itemId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(i => i.Item).FirstOrDefaultAsync(i => i.CharacterId == characterId && i.ItemId == itemId, ct);
}

public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
{
    public WalletRepository(AppDbContext db) : base(db) { }

    public Task<Wallet?> GetByCharacterIdAsync(Guid characterId, CancellationToken ct = default)
        => Set.AsNoTracking().FirstOrDefaultAsync(w => w.CharacterId == characterId, ct);
}

public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext db) : base(db) { }

    public async Task<List<Transaction>> GetByCharacterAsync(Guid characterId, int limit = 50, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(t => t.Wallet.CharacterId == characterId)
            .OrderByDescending(t => t.CreatedAt).Take(limit).ToListAsync(ct);
}

public class PlayerClanRepository : BaseRepository<PlayerClan>, IPlayerClanRepository
{
    public PlayerClanRepository(AppDbContext db) : base(db) { }

    public async Task<PlayerClan?> GetWithMembersAsync(Guid id, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(c => c.Members).Include(c => c.Wall)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PlayerClan?> GetByCharacterIdAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(c => c.Members).Include(c => c.Wall)
            .FirstOrDefaultAsync(c => c.Members.Any(m => m.CharacterId == characterId), ct);

    public async Task<List<PlayerClan>> GetRankingAsync(int limit = 100, CancellationToken ct = default)
        => await Set.AsNoTracking().OrderByDescending(c => c.Level).ThenByDescending(c => c.Xp).Take(limit).ToListAsync(ct);
}

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext db) : base(db) { }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int limit = 50, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt).Take(limit).ToListAsync(ct);
}

public class GameEventRepository : BaseRepository<GameEvent>, IGameEventRepository
{
    public GameEventRepository(AppDbContext db) : base(db) { }

    public Task<List<GameEvent>> GetActiveAsync(CancellationToken ct = default)
        => Set.AsNoTracking().Where(e => e.Status == EventStatus.Ongoing).ToListAsync(ct);
}

public class VillageRepository : BaseRepository<Village>, IVillageRepository
{
    public VillageRepository(AppDbContext db) : base(db) { }
    public async Task<Village?> GetByNameAsync(string name, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(v => v.Name == name, ct);
}

public class BloodlineClanRepository : BaseRepository<BloodlineClan>, IBloodlineClanRepository
{
    public BloodlineClanRepository(AppDbContext db) : base(db) { }
    public async Task<BloodlineClan?> GetByNameAsync(string name, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name, ct);
}
public class MissionRepository : BaseRepository<Mission>, IMissionRepository
{
    public MissionRepository(AppDbContext db) : base(db) { }

    public async Task<List<Mission>> GetAvailableForCharacterAsync(Guid characterId, CancellationToken ct = default)
        => await Db.Missions.AsNoTracking().Where(m => m.Active).ToListAsync(ct);
}
public class ItemRepository : BaseRepository<Item>, IItemRepository { public ItemRepository(AppDbContext db) : base(db) { } }
public class WorldLocationRepository : BaseRepository<WorldLocation>, IWorldLocationRepository { public WorldLocationRepository(AppDbContext db) : base(db) { } }
public class CharacterHuntRepository : BaseRepository<CharacterHunt>, ICharacterHuntRepository
{
    public CharacterHuntRepository(AppDbContext db) : base(db) { }

    public async Task<CharacterHunt?> GetActiveAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(h => h.CharacterId == characterId && !h.Completed && h.EndTime > DateTime.UtcNow, ct);

    public async Task<CharacterHunt?> GetPendingCompleteAsync(Guid characterId, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(h => h.CharacterId == characterId && !h.Completed, ct);

    public Task<int> CountTodayAsync(Guid characterId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return Set.CountAsync(h => h.CharacterId == characterId && h.CreatedAt >= today, ct);
    }
}

public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository { public AuditLogRepository(AppDbContext db) : base(db) { } }
