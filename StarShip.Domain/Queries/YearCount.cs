namespace StarShip.Domain.Queries;

/// <summary>Number of ships per build year.</summary>
public record YearCount(int Year, int Count)
{
    public override string ToString() => $"{Year}: {Count}";
}
