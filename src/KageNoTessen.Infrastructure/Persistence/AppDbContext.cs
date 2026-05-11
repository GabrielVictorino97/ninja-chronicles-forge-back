using Microsoft.EntityFrameworkCore;
using KageNoTessen.Domain;

namespace KageNoTessen.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<CharacterHunt> CharacterHunts => Set<CharacterHunt>();
    public DbSet<CharacterElement> CharacterElements => Set<CharacterElement>();
    public DbSet<CharacterAttributes> CharacterAttributes => Set<CharacterAttributes>();
    public DbSet<Village> Villages => Set<Village>();
    public DbSet<BloodlineClan> BloodlineClans => Set<BloodlineClan>();
    public DbSet<Jutsu> Jutsus => Set<Jutsu>();
    public DbSet<CharacterJutsu> CharacterJutsus => Set<CharacterJutsu>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<CharacterMission> CharacterMissions => Set<CharacterMission>();
    public DbSet<Battle> Battles => Set<Battle>();
    public DbSet<BattleParticipant> BattleParticipants => Set<BattleParticipant>();
    public DbSet<BattleLog> BattleLogs => Set<BattleLog>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<PlayerClan> PlayerClans => Set<PlayerClan>();
    public DbSet<PlayerClanMember> PlayerClanMembers => Set<PlayerClanMember>();
    public DbSet<ClanWallPost> ClanWallPosts => Set<ClanWallPost>();
    public DbSet<WorldLocation> WorldLocations => Set<WorldLocation>();
    public DbSet<GameEvent> GameEvents => Set<GameEvent>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<CharacterAchievement> CharacterAchievements => Set<CharacterAchievement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User
        builder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.Name).HasMaxLength(128);
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(32);
            e.Property(u => u.Status).HasConversion<string>().HasMaxLength(32);
        });

        // RefreshToken
        builder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(rt => rt.Token).IsUnique();
            e.Property(rt => rt.Token).HasMaxLength(256);
            e.HasOne(rt => rt.User).WithMany(u => u.RefreshTokens).HasForeignKey(rt => rt.UserId);
        });

        // UserLoginHistory
        builder.Entity<UserLoginHistory>(e =>
        {
            e.HasOne(h => h.User).WithMany(u => u.LoginHistory).HasForeignKey(h => h.UserId);
            e.Property(h => h.Ip).HasMaxLength(64);
            e.Property(h => h.Device).HasMaxLength(256);
        });

        // Village
        builder.Entity<Village>(e =>
        {
            e.HasIndex(v => v.Name).IsUnique();
            e.Property(v => v.Name).HasMaxLength(64);
            e.Property(v => v.FullName).HasMaxLength(128);
            e.Property(v => v.Country).HasMaxLength(64);
        });

        // BloodlineClan
        builder.Entity<BloodlineClan>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Name).HasMaxLength(64);
        });

        // Character
        builder.Entity<Character>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(64);
            e.Property(c => c.Avatar).HasMaxLength(16);
            e.Property(c => c.Graduation).HasConversion<string>().HasMaxLength(32);
            e.HasOne(c => c.User).WithMany(u => u.Characters).HasForeignKey(c => c.UserId);
            e.HasOne(c => c.Village).WithMany(v => v.Characters).HasForeignKey(c => c.VillageId);
            e.HasOne(c => c.Clan).WithMany(cl => cl.Characters).HasForeignKey(c => c.ClanId);
            e.HasOne(c => c.Attributes).WithOne(a => a.Character).HasForeignKey<CharacterAttributes>(a => a.CharacterId);
            e.HasOne(c => c.Wallet).WithOne(w => w.Character).HasForeignKey<Wallet>(w => w.CharacterId);
        });

        // CharacterHunt
        builder.Entity<CharacterHunt>(e =>
        {
            e.HasOne(h => h.Character).WithMany().HasForeignKey(h => h.CharacterId);
        });

        // CharacterElement
        builder.Entity<CharacterElement>(e =>
        {
            e.HasIndex(ce => new { ce.CharacterId, ce.Element }).IsUnique();
            e.Property(ce => ce.Element).HasConversion<string>().HasMaxLength(32);
            e.HasOne(ce => ce.Character).WithMany(c => c.CharacterElements).HasForeignKey(ce => ce.CharacterId);
        });

        // CharacterAttributes
        builder.Entity<CharacterAttributes>(e =>
        {
            e.ToTable("CharacterAttributes");
        });

        // Jutsu
        builder.Entity<Jutsu>(e =>
        {
            e.Property(j => j.Name).HasMaxLength(128);
            e.Property(j => j.Type).HasConversion<string>().HasMaxLength(32);
            e.Property(j => j.Element).HasConversion<string>().HasMaxLength(32);
            e.Property(j => j.MinGraduation).HasConversion<string>().HasMaxLength(32);
            e.Property(j => j.ClanRequirement).HasMaxLength(64);
            e.Property(j => j.ElementRequirement).HasConversion<string>().HasMaxLength(32);
        });

        // CharacterJutsu
        builder.Entity<CharacterJutsu>(e =>
        {
            e.HasIndex(cj => new { cj.CharacterId, cj.JutsuId }).IsUnique();
            e.HasOne(cj => cj.Character).WithMany(c => c.CharacterJutsus).HasForeignKey(cj => cj.CharacterId);
            e.HasOne(cj => cj.Jutsu).WithMany(j => j.CharacterJutsus).HasForeignKey(cj => cj.JutsuId);
        });

        // Mission
        builder.Entity<Mission>(e =>
        {
            e.Property(m => m.Title).HasMaxLength(128);
            e.Property(m => m.Rank).HasConversion<string>().HasMaxLength(8);
            e.Property(m => m.Type).HasConversion<string>().HasMaxLength(32);
            e.Property(m => m.MinGraduation).HasConversion<string>().HasMaxLength(32);
        });

        // CharacterMission
        builder.Entity<CharacterMission>(e =>
        {
            e.HasOne(cm => cm.Character).WithMany(c => c.Missions).HasForeignKey(cm => cm.CharacterId);
            e.HasOne(cm => cm.Mission).WithMany(m => m.CharacterMissions).HasForeignKey(cm => cm.MissionId);
        });

        // Battle
        builder.Entity<Battle>(e =>
        {
            e.Property(b => b.Status).HasConversion<string>().HasMaxLength(16);
            e.Property(b => b.Type).HasConversion<string>().HasMaxLength(16);
        });

        // BattleParticipant
        builder.Entity<BattleParticipant>(e =>
        {
            e.Property(bp => bp.Side).HasConversion<string>().HasMaxLength(16);
            e.HasOne(bp => bp.Battle).WithMany(b => b.Participants).HasForeignKey(bp => bp.BattleId);
        });

        // BattleLog
        builder.Entity<BattleLog>(e =>
        {
            e.Property(bl => bl.Actor).HasConversion<string>().HasMaxLength(16);
            e.HasOne(bl => bl.Battle).WithMany(b => b.Logs).HasForeignKey(bl => bl.BattleId);
        });

        // Item
        builder.Entity<Item>(e =>
        {
            e.Property(i => i.Name).HasMaxLength(128);
            e.Property(i => i.Type).HasConversion<string>().HasMaxLength(32);
            e.Property(i => i.Rarity).HasConversion<string>().HasMaxLength(16);
        });

        // InventoryItem
        builder.Entity<InventoryItem>(e =>
        {
            e.HasOne(ii => ii.Character).WithMany(c => c.Inventory).HasForeignKey(ii => ii.CharacterId);
            e.HasOne(ii => ii.Item).WithMany(i => i.Inventories).HasForeignKey(ii => ii.ItemId);
            e.Property(ii => ii.Slot).HasConversion<string>().HasMaxLength(16);
        });

        // Wallet
        builder.Entity<Wallet>(e => { e.ToTable("Wallets"); });

        // Transaction
        builder.Entity<Transaction>(e =>
        {
            e.Property(t => t.Type).HasConversion<string>().HasMaxLength(32);
            e.HasOne(t => t.Wallet).WithMany(w => w.Transactions).HasForeignKey(t => t.WalletId);
        });

        // PlayerClan
        builder.Entity<PlayerClan>(e =>
        {
            e.HasIndex(pc => pc.Name).IsUnique();
            e.Property(pc => pc.Name).HasMaxLength(64);
            e.Property(pc => pc.Tag).HasMaxLength(8);
        });

        // PlayerClanMember
        builder.Entity<PlayerClanMember>(e =>
        {
            e.HasIndex(m => new { m.ClanId, m.CharacterId }).IsUnique();
            e.Property(m => m.Role).HasConversion<string>().HasMaxLength(16);
            e.HasOne(m => m.Clan).WithMany(c => c.Members).HasForeignKey(m => m.ClanId);
        });

        // ClanWallPost
        builder.Entity<ClanWallPost>(e =>
        {
            e.HasOne(wp => wp.Clan).WithMany(c => c.Wall).HasForeignKey(wp => wp.ClanId);
        });

        // WorldLocation
        builder.Entity<WorldLocation>(e =>
        {
            e.Property(l => l.Name).HasMaxLength(128);
            e.Property(l => l.Type).HasConversion<string>().HasMaxLength(32);
            e.Property(l => l.GraduationRequired).HasConversion<string>().HasMaxLength(32);
        });

        // GameEvent
        builder.Entity<GameEvent>(e =>
        {
            e.Property(ge => ge.Type).HasConversion<string>().HasMaxLength(32);
            e.Property(ge => ge.Status).HasConversion<string>().HasMaxLength(16);
        });

        // Achievement
        builder.Entity<Achievement>(e => { e.Property(a => a.Name).HasMaxLength(128); });

        // CharacterAchievement
        builder.Entity<CharacterAchievement>(e =>
        {
            e.HasOne(ca => ca.Character).WithMany(c => c.Achievements).HasForeignKey(ca => ca.CharacterId);
            e.HasOne(ca => ca.Achievement).WithMany().HasForeignKey(ca => ca.AchievementId);
        });

        // Notification
        builder.Entity<Notification>(e =>
        {
            e.Property(n => n.Type).HasConversion<string>().HasMaxLength(16);
            e.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId);
        });

        // AuditLog
        builder.Entity<AuditLog>(e =>
        {
            e.Property(al => al.Action).HasMaxLength(64);
            e.Property(al => al.Entity).HasMaxLength(64);
            e.Property(al => al.Ip).HasMaxLength(64);
        });
    }
}
