using FluentValidation;
using MediatR;
using NarutoPlayers.Application.Interfaces;
using NarutoPlayers.Contracts.Characters;
using NarutoPlayers.Domain;

namespace NarutoPlayers.Application.Characters;

public record CreateCharacterCommand(
    Guid UserId, string Name, string Avatar, Guid VillageId,
    Guid ClanId) : IRequest<CharacterDto>;

public record UpdateAttributesCommand(
    Guid CharacterId, Dictionary<string, int> Attributes) : IRequest<CharacterDto>;

public record GetMyCharacterQuery(Guid UserId) : IRequest<CharacterDto?>;

public record GetCharacterQuery(Guid CharacterId) : IRequest<CharacterDto?>;

public class CreateCharacterValidator : AbstractValidator<CreateCharacterCommand>
{
    public CreateCharacterValidator(ICharacterRepository characters)
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(32);
        RuleFor(x => x.Avatar).NotEmpty();
    }
}

public class CharacterHandler :
    IRequestHandler<CreateCharacterCommand, CharacterDto>,
    IRequestHandler<UpdateAttributesCommand, CharacterDto>,
    IRequestHandler<GetMyCharacterQuery, CharacterDto?>,
    IRequestHandler<GetCharacterQuery, CharacterDto?>
{
    private readonly ICharacterRepository _characters;
    private readonly ICharacterJutsuRepository _charJutsu;

    public CharacterHandler(ICharacterRepository characters, ICharacterJutsuRepository charJutsu)
    {
        _characters = characters;
        _charJutsu = charJutsu;
    }

    public async Task<CharacterDto> Handle(CreateCharacterCommand cmd, CancellationToken ct)
    {
        var character = Character.Create(cmd.UserId, cmd.Name, cmd.Avatar,
            cmd.VillageId, cmd.ClanId);
        await _characters.AddAsync(character, ct);
        return Map(character);
    }

    public async Task<CharacterDto> Handle(UpdateAttributesCommand cmd, CancellationToken ct)
    {
        var c = await _characters.GetWithDetailsAsync(cmd.CharacterId, ct)
            ?? throw new InvalidOperationException("Character not found");

        if (c.UnspentPoints <= 0) throw new InvalidOperationException("No unspent points");

        var attr = c.Attributes;
        var changes = new Dictionary<string, int>
        {
            ["taijutsu"] = 0, ["ninjutsu"] = 0, ["genjutsu"] = 0,
            ["intelligence"] = 0, ["vitality"] = 0, ["chakra"] = 0,
            ["agility"] = 0, ["luck"] = 0,
        };

        foreach (var (key, value) in cmd.Attributes)
        {
            var k = key.ToLowerInvariant();
            if (!changes.ContainsKey(k)) continue;
            var diff = value - GetAttr(attr, k);
            changes[k] = diff;
        }

        var total = changes.Values.Sum();
        if (total > c.UnspentPoints) throw new InvalidOperationException("Not enough points");
        if (changes.Values.Any(d => d < 0)) throw new InvalidOperationException("Cannot decrease attributes");

        attr.Taijutsu += changes["taijutsu"];
        attr.Ninjutsu += changes["ninjutsu"];
        attr.Genjutsu += changes["genjutsu"];
        attr.Intelligence += changes["intelligence"];
        attr.Vitality += changes["vitality"];
        attr.Chakra += changes["chakra"];
        attr.Agility += changes["agility"];
        attr.Luck += changes["luck"];
        c.SpendPoints(total);
        c.ApplyDerivedAttributes();
        await _characters.UpdateAsync(c, ct);

        return Map(c);
    }

    public async Task<CharacterDto?> Handle(GetMyCharacterQuery query, CancellationToken ct)
    {
        var c = await _characters.GetByUserIdAsync(query.UserId, ct);
        return c is null ? null : Map(c);
    }

    public async Task<CharacterDto?> Handle(GetCharacterQuery query, CancellationToken ct)
    {
        var c = await _characters.GetWithDetailsAsync(query.CharacterId, ct);
        return c is null ? null : Map(c);
    }

    private static int GetAttr(CharacterAttributes a, string key) => key switch
    {
        "taijutsu" => a.Taijutsu, "ninjutsu" => a.Ninjutsu,
        "genjutsu" => a.Genjutsu, "intelligence" => a.Intelligence,
        "vitality" => a.Vitality, "chakra" => a.Chakra,
        "agility" => a.Agility, "luck" => a.Luck,
        _ => throw new ArgumentException($"Unknown attribute: {key}")
    };

    private CharacterDto Map(Character c) => new(
        c.Id.ToString(), c.UserId.ToString(), c.Name, c.Avatar,
        c.VillageId.ToString(), c.ClanId.ToString(),
        c.CharacterElements.Select(e => e.Element.ToString()).ToArray(),
        c.Graduation.ToString(), c.Level, c.Xp, c.XpToNext,
        c.Hp, c.HpMax, c.Chakra, c.ChakraMax,
        c.Energy, c.EnergyMax, c.Ryous, c.Power,
        new AttributeDto(c.Attributes.Taijutsu, c.Attributes.Ninjutsu, c.Attributes.Genjutsu,
            c.Attributes.Intelligence, c.Attributes.Vitality, c.Attributes.Chakra,
            c.Attributes.Agility, c.Attributes.Luck),
        c.UnspentPoints,
        Array.Empty<string>(),
        Array.Empty<string>(),
        c.CreatedAt);
}
