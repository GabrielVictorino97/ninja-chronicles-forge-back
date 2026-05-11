using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Characters;
using KageNoTessen.Domain;

namespace KageNoTessen.Api.Endpoints;

public class GetHuntStatusEndpoint : EndpointWithoutRequest<HuntStatusDto>
{
    public override void Configure()
    {
        Get("hunts/status");
        Description(d => d
            .WithName("StatusCacada")
            .WithSummary("Retorna o status da caçada atual, tempo disponível hoje e as durações disponíveis"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var maxDuration = Math.Min(50, ((c.Level - 1) / 5 + 1) * 5);
        var baseDurations = Enumerable.Range(1, maxDuration / 5).Select(i => i * 5);

        var repo = Resolve<ICharacterHuntRepository>();
        var todayCount = await repo.CountTodayAsync(c.Id, ct);
        var todayRemaining = Math.Max(0, 10 - todayCount);
        var totalAvailableMinutes = todayRemaining * maxDuration;

        var availableDurations = baseDurations
            .Where(d => d <= totalAvailableMinutes)
            .ToArray();

        var hunt = await repo.GetActiveAsync(c.Id, ct);
        if (hunt is null)
        {
            await SendOkAsync(new HuntStatusDto(false, 0, 0, 0, 0,
                DateTime.MinValue, DateTime.MinValue, 0, availableDurations,
                todayCount, todayRemaining, totalAvailableMinutes), ct);
            return;
        }

        var remaining = (int)(hunt.EndTime - DateTime.UtcNow).TotalSeconds;
        await SendOkAsync(new HuntStatusDto(true, hunt.HuntLevel, hunt.DurationMinutes,
            hunt.XpReward, hunt.RyousReward,
            hunt.StartTime, hunt.EndTime, Math.Max(0, remaining),
            availableDurations,
            todayCount, todayRemaining, totalAvailableMinutes), ct);
    }
}

public class StartHuntEndpoint : Endpoint<StartHuntRequest>
{
    public override void Configure()
    {
        Post("hunts/start");
        Description(d => d
            .WithName("IniciarCacada")
            .WithSummary("Inicia uma caçada. Duração em minutos (5-50, múltiplos de 5). Caçadas mais curtas têm bônus de recompensa.")
            .Accepts<StartHuntRequest>("application/json"));
    }

    public override async Task HandleAsync(StartHuntRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        if (req.DurationMinutes < 5 || req.DurationMinutes > 50 || req.DurationMinutes % 5 != 0)
        {
            AddError("duration", "Duracao deve ser multiplo de 5, entre 5 e 50 minutos.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var huntRepo = Resolve<ICharacterHuntRepository>();
        if (await huntRepo.GetActiveAsync(c.Id, ct) is not null)
        {
            AddError("hunt", "Voce ja esta em uma cacada. Conclua ou aguarde o tempo expirar.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var todayCount = await huntRepo.CountTodayAsync(c.Id, ct);
        if (todayCount >= 10)
        {
            AddError("hunt", "Limite diario de 10 cacadas atingido. Volte amanha.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var maxDuration = Math.Min(50, ((c.Level - 1) / 5 + 1) * 5);
        var totalAvailableMinutes = (10 - todayCount) * maxDuration;
        if (req.DurationMinutes > totalAvailableMinutes)
        {
            AddError("duration", $"Duracao excede o tempo total disponivel hoje ({totalAvailableMinutes} minutos). Restam {10 - todayCount} cacada(s).");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var blocks = req.DurationMinutes / 5;
        var huntLevel = Math.Min(10, (c.Level - 1) / 5 + 1);
        var rng = new Random();

        // Base reward per 5min block (random between min and max based on hunt level)
        var xpPerBlock = rng.Next(huntLevel * 20, huntLevel * 40 + 1);
        var ryousPerBlock = rng.Next(huntLevel * 30, huntLevel * 70 + 1);

        // Bonus multiplier: shorter hunts give better rewards per minute
        var bonus = blocks switch
        {
            1 => 1.5,   //  5 min → +50%
            2 => 1.3,   // 10 min → +30%
            3 => 1.15,  // 15 min → +15%
            4 => 1.05,  // 20 min → +5%
            _ => 1.0,   // 25+ min → base
        };

        var totalXp = (int)(xpPerBlock * blocks * bonus);
        var totalRyous = (int)(ryousPerBlock * blocks * bonus);

        var hunt = CharacterHunt.Start(c.Id, huntLevel, req.DurationMinutes, totalXp, totalRyous);
        await huntRepo.AddAsync(hunt, ct);
        await SendOkAsync(ct);
    }
}

public class CompleteHuntEndpoint : EndpointWithoutRequest<HuntRewardDto>
{
    public override void Configure()
    {
        Post("hunts/complete");
        Description(d => d
            .WithName("ConcluirCacada")
            .WithSummary("Conclui a caçada ativa se o tempo tiver passado e recebe as recompensas"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var charRepo = Resolve<ICharacterRepository>();
        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null) { await SendNotFoundAsync(ct); return; }

        var huntRepo = Resolve<ICharacterHuntRepository>();
        var hunt = await huntRepo.GetActiveAsync(c.Id, ct);
        if (hunt is null)
        {
            AddError("hunt", "Nenhuma cacada ativa. Use POST /api/hunts/start para iniciar.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!hunt.IsExpired())
        {
            var remaining = (int)(hunt.EndTime - DateTime.UtcNow).TotalSeconds;
            AddError("time", $"A cacada ainda esta em andamento. Aguarde {remaining / 60}min {remaining % 60}s.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        hunt.Complete();
        await huntRepo.UpdateAsync(hunt, ct);

        c.AddXp(hunt.XpReward);
        c.AddRyous(hunt.RyousReward);
        await charRepo.UpdateAsync(c, ct);

        await SendOkAsync(new HuntRewardDto(hunt.XpReward, hunt.RyousReward, hunt.DurationMinutes), ct);
    }
}
