using StarShip.Domain.Models.Shared;

namespace StarShip.Domain.Queries;

/// <summary>Number of crew members per role.</summary>
public record RoleCount(CrewRole Role, int Count)
{
    public override string ToString() => $"{Role}: {Count}";
}
