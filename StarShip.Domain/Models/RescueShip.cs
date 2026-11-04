using StarShip.Domain.Exceptions;
using StarShip.Domain.Models.Shared;

namespace StarShip.Domain.Models;

public class RescueShip : BaseShip
{
    // --- Constants ---

    private static readonly IReadOnlyDictionary<CrewRole, int> Roles =
        new Dictionary<CrewRole, int> { [CrewRole.Captain] = 1, [CrewRole.Pilot] = 1, [CrewRole.Medic] = 1 };

    // --- Ctor ---

    public RescueShip(string name, Coordinate position) : base(name, position, Roles)
    {
    }

    // --- Queries (Akt 4) ---

    /// <summary>True if all five rescue rules hold.</summary>
    public bool CanRescue(BaseShip target, SpaceStation home) =>
        RescueBlocker(target, home) is null;

    // LINQ: Where
    /// <summary>The scan of the base, only ships in distress.</summary>
    public override IEnumerable<BaseShip> Scan(IEnumerable<BaseShip> ships) =>
        base.Scan(ships).Where(s => s.Status == ShipStatus.Distress);

    // --- Business Methods (Akt 4) ---

    /// <summary>Takes over the crew of a ship in distress, saves its logbook and clears the signal.</summary>
    public void Rescue(BaseShip target, SpaceStation home)
    {
        // Guard: target and home must not be null
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(home);

        // Business Rules: all five rules before the first change
        string? blocker = RescueBlocker(target, home);
        Rules.Require(blocker is null, blocker ?? string.Empty);
        var signal = target.Distress ?? throw new StarshipException($"{target.Name} sendet keinen Notruf.");

        // Business Logic: crew over, logbook saved, signal cleared
        var rescued = target.Crew.ToList();
        foreach (var member in rescued)
        {
            target.Unboard(member);
            Board(member);
        }
        Logbook.Append(target.Logbook);
        target.ClearDistressSignal();

        // Audit Log
        Logbook.Add(LogType.Rescue,
            $"Crew von {target.Name} übernommen ({rescued.Count} Personen), Notruf von {signal.SentAt:HH:mm:ss} beantwortet");
    }

    // --- Business Methods (Story) ---

    /// <summary>Takes an injured crew member over from a connected ship, a medic must be on board.</summary>
    public void TakeInjured(BaseShip from, CrewMember member)
    {
        // Guard: from and member must not be null
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(member);

        // Business Rule: a medic on board who can treat the injured
        Rules.Require(HasRequiredRoles, $"{Name} fehlt Crew: {string.Join(", ", MissingRoles())}.");

        // Business Logic: over the rescue tube (Transfer checks position and seats)
        from.Transfer(member, this);

        // Audit Log
        Logbook.Add(LogType.Rescue, $"{member} verletzt übernommen, auf der Krankenstation");
    }

    // --- Helpers ---

    /// <summary>The message of the first broken rescue rule, or null if all rules hold.</summary>
    private string? RescueBlocker(BaseShip target, SpaceStation home)
    {
        if (target.Status != ShipStatus.Distress)
            return $"{target.Name} ist nicht in Not.";
        if (Position != target.Position)
            return $"{Name} ist nicht bei {target.Name}.";
        if (!HasRequiredRoles)
            return $"{Name} fehlt Crew: {string.Join(", ", MissingRoles())}.";
        if (CrewCount + target.CrewCount > MaxCrew)
            return $"{Name} hat nicht genug Sitze für die Crew von {target.Name}.";
        if (!HasEnoughFuelTo(home.Position))
            return $"Die Antimaterie von {Name} reicht nicht zurück bis {home.Name}.";
        return null;
    }
}
