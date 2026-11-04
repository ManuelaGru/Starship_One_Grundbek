namespace StarShip.Domain.Queries;

/// <summary>Number of docked ships per type.</summary>
public record TypeCount(string Type, int Count)
{
    public override string ToString() => $"{Type}: {Count}";
}
