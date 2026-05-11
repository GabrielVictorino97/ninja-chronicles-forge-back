using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NarutoPlayers.Application.Interfaces;
using NarutoPlayers.Infrastructure.Persistence;
using NarutoPlayers.Infrastructure.Persistence.Repositories;
using NarutoPlayers.Infrastructure.Services;
using NarutoPlayers.Domain;

namespace NarutoPlayers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("Default")));

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IJwtService, JwtService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IVillageRepository, VillageRepository>();
        services.AddScoped<IBloodlineClanRepository, BloodlineClanRepository>();
        services.AddScoped<IJutsuRepository, JutsuRepository>();
        services.AddScoped<ICharacterElementRepository, CharacterElementRepository>();
        services.AddScoped<ICharacterHuntRepository, CharacterHuntRepository>();
        services.AddScoped<ICharacterJutsuRepository, CharacterJutsuRepository>();
        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<ICharacterMissionRepository, CharacterMissionRepository>();
        services.AddScoped<IBattleRepository, BattleRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPlayerClanRepository, PlayerClanRepository>();
        services.AddScoped<IWorldLocationRepository, WorldLocationRepository>();
        services.AddScoped<IGameEventRepository, GameEventRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<BaseRepository<ClanWallPost>>();
        services.AddScoped<BaseRepository<Achievement>>();
        services.AddScoped<BaseRepository<CharacterAchievement>>();
        services.AddScoped<BaseRepository<UserLoginHistory>>();

        return services;
    }
}
