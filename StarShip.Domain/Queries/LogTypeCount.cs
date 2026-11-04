using StarShip.Domain.Models.Shared;

namespace StarShip.Domain.Queries;

/// <summary>Number of log entries per type.</summary>
public record LogTypeCount(LogType Type, int Count)
{
    public override string ToString() => $"{Type}: {Count}";
}
