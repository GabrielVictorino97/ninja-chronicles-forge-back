using FluentValidation;
using MediatR;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Characters;
using KageNoTessen.Domain;

namespace KageNoTessen.Application.Characters;

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
        await _characters.LoadReferencesAsync(character, ct);
        return Map(character);
    }

    public async Task<CharacterDto> Handle(UpdateAttributesCommand cmd, CancellationToken ct)
    {
        var c = await _characters.GetWithDetailsAsync(cmd.CharacterId, ct)
            ?? throw new InvalidOperationException("Character not found");

        if (c.UnspentPoints <= 0) throw new InvalidOperationException("No unspent points");

        var validKeys = new HashSet<string> { "taijutsu", "ninjutsu", "genjutsu", "intelligence", "vitality", "chakra", "agility", "luck" };
        var attr = c.Attributes;

        foreach (var (key, value) in cmd.Attributes)
        {
            if (!validKeys.Contains(key.ToLowerInvariant()))
                throw new InvalidOperationException($"Invalid attribute: {key}");
            if (value < 0)
                throw new InvalidOperationException("Cannot decrease attributes");
        }

        var total = cmd.Attributes.Values.Sum();
        if (total > c.UnspentPoints) throw new InvalidOperationException("Not enough points");

        foreach (var (key, value) in cmd.Attributes)
        {
            if (value == 0) continue;
            _ = key.ToLowerInvariant() switch
            {
                "taijutsu" => attr.Taijutsu += value,
                "ninjutsu" => attr.Ninjutsu += value,
                "genjutsu" => attr.Genjutsu += value,
                "intelligence" => attr.Intelligence += value,
                "vitality" => attr.Vitality += value,
                "chakra" => attr.Chakra += value,
                "agility" => attr.Agility += value,
                "luck" => attr.Luck += value,
                _ => 0
            };
        }

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

    private CharacterDto Map(Character c) => new(
        c.Id.ToString(), c.UserId.ToString(), c.Name, c.Avatar,
        c.Village.Name.ToLower(), c.Clan.Name.ToLower(),
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
