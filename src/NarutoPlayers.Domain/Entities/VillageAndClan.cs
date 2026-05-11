namespace NarutoPlayers.Domain;

public class Village : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Symbol { get; private set; } = null!;
    public string AccentColor { get; private set; } = null!;
    public bool Active { get; set; } = true;
    public string Bonus { get; set; } = "";

    public ICollection<Character> Characters { get; private set; } = new List<Character>();

    private Village() { }

    public static Village Create(string name, string fullName, string country, string description, string symbol, string accentColor)
        => new() { Name = name, FullName = fullName, Country = country, Description = description, Symbol = symbol, AccentColor = accentColor };
}

public class BloodlineClan : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Bonus { get; private set; } = null!;
    public string Symbol { get; private set; } = null!;
    public string? VillageOrigin { get; set; }
    public string? KekkeiGenkai { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<Character> Characters { get; private set; } = new List<Character>();

    private BloodlineClan() { }

    public static BloodlineClan Create(string name, string description, string bonus, string symbol)
        => new() { Name = name, Description = description, Bonus = bonus, Symbol = symbol };
}
