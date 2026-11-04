namespace StarShip.Domain.Models.Shared;

public record CrewMember
{
    // --- Props ---

    public string Name { get; init; }
    public CrewRole Role { get; init; }

    // --- Ctor ---

    public CrewMember(string name, CrewRole role)
    {
        // Guard: name must not be empty
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Role = role;
    }

    // --- Factory Methods ---

    /// <summary>Creates a captain with the given name.</summary>
    public static CrewMember Captain(string name) => new(name, CrewRole.Captain);

    /// <summary>Creates a pilot with the given name.</summary>
    public static CrewMember Pilot(string name) => new(name, CrewRole.Pilot);

    /// <summary>Creates an engineer with the given name.</summary>
    public static CrewMember Engineer(string name) => new(name, CrewRole.Engineer);

    /// <summary>Creates a medic with the given name.</summary>
    public static CrewMember Medic(string name) => new(name, CrewRole.Medic);

    // --- Output ---

    /// <summary>Formats the member as Name (Role), e.g. Kira (Captain).</summary>
    public override string ToString() => $"{Name} ({Role})";
}
