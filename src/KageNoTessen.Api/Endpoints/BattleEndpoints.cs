using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using KageNoTessen.Application.Battle;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Battle;
using Microsoft.Extensions.Options;

namespace KageNoTessen.Api.Endpoints;

public class NpcBattleEndpoint : Endpoint<NpcBattleRequest, BattleResultDto>
{
    public override void Configure()
    {
        Post("characters/{characterId:guid}/battles/npc");
        Description(d => d
            .WithName("BatalhaNPC")
            .WithSummary("Inicia uma batalha instantanea contra NPC. O resultado considera nivel, atributos, jutsus, itens e elementos.")
            .Accepts<NpcBattleRequest>("application/json"));
    }

    public override async Task HandleAsync(NpcBattleRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var calculator = Resolve<CombatPowerCalculator>();
        var options = Resolve<IOptions<CombatBalanceOptions>>().Value;

        var c = await charRepo.GetByUserIdAsync(userId, ct);
        if (c is null || c.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var huntRepo = Resolve<ICharacterHuntRepository>();
        if (await huntRepo.GetActiveAsync(c.Id, ct) is not null)
        {
            AddError("hunt", "Voce esta em uma cacada. Conclua a cacada primeiro antes de batalhar.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (req.Difficulty is not ("easy" or "normal" or "hard"))
        {
            AddError("difficulty", "Dificuldade invalida. Use: easy, normal ou hard.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var rng = new Random();
        var (npcLevel, difficultyLabel) = req.Difficulty switch
        {
            "easy" => (Math.Max(1, c.Level - rng.Next(1, 11)), "Facil"),
            "hard" => (c.Level + rng.Next(1, 11), "Dificil"),
            _ => (Math.Max(1, c.Level + rng.Next(-3, 4)), "Normal")
        };

        var playerPower = calculator.Calculate(c);
        var npcPower = calculator.CalculateNpcPower(npcLevel, req.Difficulty);
        var (winChance, powerComparison) = calculator.GetWinChance(playerPower, npcPower);

        var enemyNames = new[] { "Bandido", "Ninja Renegado", "Missing-nin", "Espiao da Akatsuki",
            "Mercenario", "Assassino", "Forasteiro", "Ronin" };
        var enemyName = enemyNames[rng.Next(enemyNames.Length)];

        if (rng.Next(100) < winChance)
        {
            var (xpReward, ryousReward) = req.Difficulty switch
            {
                "easy" => (npcLevel * 5 + rng.Next(0, npcLevel * 3),
                           npcLevel * 8 + rng.Next(0, npcLevel * 5)),
                "hard" => (npcLevel * 18 + rng.Next(0, npcLevel * 8),
                           npcLevel * 25 + rng.Next(0, npcLevel * 12)),
                _ => (npcLevel * 10 + rng.Next(0, npcLevel * 5),
                      npcLevel * 15 + rng.Next(0, npcLevel * 8))
            };

            c.AddXp(xpReward);
            c.AddRyous(ryousReward);
            await charRepo.UpdateAsync(c, ct);

            await SendOkAsync(new BattleResultDto("Vitoria", enemyName, npcLevel,
                difficultyLabel, xpReward, ryousReward, c.Level, c.Graduation.ToString(),
                playerPower, npcPower, powerComparison), ct);
        }
        else
        {
            var lossPercent = rng.Next((int)options.NpcLossMinPercent, (int)options.NpcLossMaxPercent + 1) / 100.0;
            var ryousLost = (int)(c.Ryous * lossPercent);
            c.LoseRyous(ryousLost);
            await charRepo.UpdateAsync(c, ct);

            await SendOkAsync(new BattleResultDto("Derrota", enemyName, npcLevel,
                difficultyLabel, 0, -ryousLost, c.Level, c.Graduation.ToString(),
                playerPower, npcPower, powerComparison), ct);
        }
    }
}

public class PvpBattleEndpoint : Endpoint<PvpBattleRequest, BattleResultDto>
{
    public override void Configure()
    {
        Post("characters/{characterId:guid}/battles/pvp");
        Description(d => d
            .WithName("BatalhaPvP")
            .WithSummary("Ataca outro jogador pelo nome do personagem. O resultado considera poder de combate de ambos.")
            .Accepts<PvpBattleRequest>("application/json"));
    }

    public override async Task HandleAsync(PvpBattleRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var characterId = Route<Guid>("characterId");
        var charRepo = Resolve<ICharacterRepository>();
        var calculator = Resolve<CombatPowerCalculator>();
        var options = Resolve<IOptions<CombatBalanceOptions>>().Value;

        var attacker = await charRepo.GetByUserIdAsync(userId, ct);
        if (attacker is null || attacker.Id != characterId)
        { AddError("character", "Personagem nao encontrado ou nao pertence a sua conta."); await SendErrorsAsync(cancellation: ct); return; }

        var huntRepo = Resolve<ICharacterHuntRepository>();
        if (await huntRepo.GetActiveAsync(attacker.Id, ct) is not null)
        {
            AddError("hunt", "Voce esta em uma cacada. Conclua a cacada primeiro antes de batalhar.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(req.TargetName))
        {
            AddError("targetName", "Informe o nome do personagem alvo.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var target = await charRepo.GetByNameAsync(req.TargetName.Trim(), ct);
        if (target is null)
        {
            AddError("targetName", $"Personagem '{req.TargetName}' nao encontrado.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (target.Id == attacker.Id)
        {
            AddError("targetName", "Voce nao pode atacar seu proprio personagem.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (target.LastPvPAttackedAt.HasValue
            && (DateTime.UtcNow - target.LastPvPAttackedAt.Value).TotalMinutes < options.PvpCooldownMinutes)
        {
            var remaining = options.PvpCooldownMinutes - (int)(DateTime.UtcNow - target.LastPvPAttackedAt.Value).TotalMinutes;
            AddError("cooldown", $"Esse jogador foi atacado recentemente. Aguarde {remaining}min {60 - (int)(DateTime.UtcNow - target.LastPvPAttackedAt.Value).TotalSeconds % 60}s.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var attackerPower = calculator.Calculate(attacker);
        var targetPower = calculator.Calculate(target);
        var (winChance, powerComparison) = calculator.GetWinChance(attackerPower, targetPower);

        var levelDiff = target.Level - attacker.Level;
        var difficultyLabel = levelDiff switch
        {
            <= -10 => "Facil",
            >= 10 => "Dificil",
            _ => "Normal"
        };

        var rng = new Random();

        if (rng.Next(100) < winChance)
        {
            var xpReward = levelDiff switch
            {
                <= -10 => target.Level * 5 + rng.Next(0, target.Level * 3),
                >= 10 => target.Level * 18 + rng.Next(0, target.Level * 8),
                _ => target.Level * 10 + rng.Next(0, target.Level * 5)
            };

            var ryousPercent = rng.Next(options.PvpRyousStealMin, options.PvpRyousStealMax + 1) / 100.0;
            var ryousStolen = (int)(target.Ryous * ryousPercent);

            target.LastPvPAttackedAt = DateTime.UtcNow;
            target.LoseRyous(ryousStolen);
            await charRepo.UpdateAsync(target, ct);

            attacker.AddXp(xpReward);
            attacker.AddRyous(ryousStolen);
            await charRepo.UpdateAsync(attacker, ct);

            await SendOkAsync(new BattleResultDto("Vitoria", target.Name, target.Level,
                difficultyLabel, xpReward, ryousStolen, attacker.Level, attacker.Graduation.ToString(),
                attackerPower, targetPower, powerComparison), ct);
        }
        else
        {
            var lossPercent = rng.Next((int)options.PvpLossMinPercent, (int)options.PvpLossMaxPercent + 1) / 100.0;
            var ryousLost = (int)(attacker.Ryous * lossPercent);

            target.LastPvPAttackedAt = DateTime.UtcNow;
            await charRepo.UpdateAsync(target, ct);

            attacker.LoseRyous(ryousLost);
            await charRepo.UpdateAsync(attacker, ct);

            await SendOkAsync(new BattleResultDto("Derrota", target.Name, target.Level,
                difficultyLabel, 0, -ryousLost, attacker.Level, attacker.Graduation.ToString(),
                attackerPower, targetPower, powerComparison), ct);
        }
    }
}
