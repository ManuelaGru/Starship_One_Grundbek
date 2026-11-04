using StarShip.Domain.Models.Shared;

namespace StarShip.Domain.Models;

/// <summary>Story: small, light scout ship, jumps twice as far, a captain flies it alone.</summary>
public class ScoutShip : BaseShip
{
    // --- Constants ---

    public const int ScoutRange = 2 * WarpRange;

    private static readonly IReadOnlyDictionary<CrewRole, int> Roles =
        new Dictionary<CrewRole, int> { [CrewRole.Captain] = 1 };

    // --- Queries ---

    public override int JumpRange => ScoutRange;

    // --- Ctor ---

    public ScoutShip(string name, Coordinate position) : base(name, position, Roles)
    {
    }
}
