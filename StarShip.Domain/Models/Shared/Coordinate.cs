namespace StarShip.Domain.Models.Shared;

public record Coordinate(int X, int Y)
{
    // --- Queries ---

    /// <summary>Distance to another point in light years, not rounded (Pythagoras).</summary>
    public double DistanceTo(Coordinate other)
    {
        // Guard: other must not be null
        ArgumentNullException.ThrowIfNull(other);

        int dx = other.X - X;
        int dy = other.Y - Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // --- Output ---

    /// <summary>Formats the coordinate as (X,Y) Lichtjahre.</summary>
    public override string ToString() => $"({X},{Y}) Lichtjahre";
}
