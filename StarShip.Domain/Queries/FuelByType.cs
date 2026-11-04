namespace StarShip.Domain.Queries;

/// <summary>Sum of antimatter per ship type.</summary>
public record FuelByType(string Type, int Fuel)
{
    public override string ToString() => $"{Type}: {Fuel}";
}
