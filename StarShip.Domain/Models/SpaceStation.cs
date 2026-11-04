using StarShip.Domain.Exceptions;
using StarShip.Domain.Models.Shared;
using StarShip.Domain.Queries;

namespace StarShip.Domain.Models;

public class SpaceStation : BaseEntity
{
    // --- Constants ---

    public const int MaxDocks = 10;

    // --- Fields ---

    private readonly List<BaseShip> _ships = [];         // Invariante: höchstens MaxDocks, kein Schiff doppelt, jedes steht bei Position
    private readonly List<CrewMember> _crewPool = [];    // Invariante: niemand doppelt

    // --- Props ---

    public int FuelPool { get; private set; }            // Invariante: nie negativ

    // --- Queries ---

    public IReadOnlyList<BaseShip> Ships => _ships.AsReadOnly();
    public IReadOnlyList<CrewMember> CrewPool => _crewPool.AsReadOnly();
    public int ShipCount => _ships.Count;
    public int CrewCount => _crewPool.Count;

    // LINQ: Count
    /// <summary>How many members with this role wait in the pool.</summary>
    public int CrewCountByRole(CrewRole role) =>
        _crewPool.Count(c => c.Role == role);

    // LINQ: Select, ToList
    /// <summary>The names of all members in pool order.</summary>
    public IReadOnlyList<string> CrewNames() =>
        _crewPool.Select(c => c.Name).ToList();

    // LINQ: GroupBy, Select, OrderBy
    /// <summary>Number of members per role, ordered by role.</summary>
    public IReadOnlyList<RoleCount> CrewCountPerRole() =>
        _crewPool
            .GroupBy(c => c.Role)
            .Select(g => new RoleCount(g.Key, g.Count()))
            .OrderBy(r => r.Role)
            .ToList();

    // LINQ: All
    /// <summary>True if every docked ship is ready (Akt 5).</summary>
    public bool AllShipsReady() =>
        _ships.All(s => s.Readiness == Readiness.Ready);

    // LINQ: Any
    /// <summary>True if at least one docked ship is ready (Akt 5).</summary>
    public bool AnyShipReady() =>
        _ships.Any(s => s.Readiness == Readiness.Ready);

    // LINQ: GroupBy, Select, ToList
    /// <summary>Number of docked ships per type name, in dock order (Akt 5).</summary>
    public IReadOnlyList<TypeCount> ShipCountByType() =>
        _ships
            .GroupBy(s => s.GetType().Name)
            .Select(g => new TypeCount(g.Key, g.Count()))
            .ToList();

    // --- Ctor ---

    public SpaceStation(string name, Coordinate position) : base(name, position)
    {
    }

    // --- Business Methods (Akt 1) ---

    /// <summary>Adds a crew member to the crew pool of the station.</summary>
    public void AddCrew(CrewMember member)
    {
        // Guard: member must not be null
        ArgumentNullException.ThrowIfNull(member);

        // Business Rule: not in the pool yet
        Rules.Require(!_crewPool.Contains(member), $"{member.Name} ist schon im Pool von {Name}.");

        // Business Logic: add the member to the pool
        _crewPool.Add(member);

        // Audit Log
        Logbook.Add(LogType.Crew, $"{member} im Pool");
    }

    /// <summary>Takes a delivery of antimatter into the pool.</summary>
    public void Supply(int amount)
    {
        // Guard: amount must be positive
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        // Business Logic: the pool grows
        FuelPool += amount;

        // Audit Log
        Logbook.Add(LogType.Fuel, $"{amount} Einheiten Antimaterie geliefert");
    }

    /// <summary>Docks a ship and sets both sides of the relation, parent first.</summary>
    public void Dock(BaseShip ship)
    {
        // Guard: ship must not be null
        ArgumentNullException.ThrowIfNull(ship);

        // Business Rules: a free dock, not here yet, same position, not docked elsewhere
        Rules.Require(ShipCount < MaxDocks, "Kein Pylon frei.");
        Rules.Require(!_ships.Contains(ship), $"{ship.Name} ist schon an {Name} angedockt.");
        Rules.Require(ship.Position == Position, $"{ship.Name} ist nicht bei {Name}.");
        Rules.Require(!ship.IsDocked, $"{ship.Name} hängt schon an {ship.Station?.Name}.");
        Rules.Require(ship.Distress is null, $"{ship.Name} sendet einen Notruf und kann nicht andocken.");   // Invariante: Notruf nur im All

        // Business Logic: parent side first, then child side
        _ships.Add(ship);
        ship.AttachTo(this);

        // Audit Log
        Logbook.Add(LogType.Docking, $"{ship.Name} angedockt");
    }

    /// <summary>Undocks a ship, reverse order: parent side first, then child side.</summary>
    public void Undock(BaseShip ship)
    {
        // Guard: ship must not be null
        ArgumentNullException.ThrowIfNull(ship);

        // Business Rule: docked here
        Rules.Require(_ships.Contains(ship), $"{ship.Name} ist nicht an {Name} angedockt.");

        // Business Logic: remove from the list, then detach
        _ships.Remove(ship);
        ship.Detach();

        // Audit Log
        Logbook.Add(LogType.Docking, $"{ship.Name} abgedockt");
    }

    /// <summary>Fills the warp core of a docked ship completely from the pool.</summary>
    public void Refuel(BaseShip ship)
    {
        // Guard: ship must not be null
        ArgumentNullException.ThrowIfNull(ship);

        // Business Rules: docked here, warp core not full, pool big enough
        Rules.Require(_ships.Contains(ship), $"{ship.Name} ist nicht an {Name} angedockt.");
        Rules.Require(ship.Fuel < BaseShip.MaxFuel, $"Der Warpkern von {ship.Name} ist schon voll.");
        int missing = BaseShip.MaxFuel - ship.Fuel;
        Rules.Require(FuelPool >= missing, $"{Name} hat nur {FuelPool} Antimaterie, {ship.Name} braucht {missing}.");

        // Business Logic: from the pool into the warp core
        ship.AddFuel(missing);
        FuelPool -= missing;

        // Audit Log
        Logbook.Add(LogType.Fuel, $"{ship.Name} geladen mit {missing}, Pool {FuelPool}");
    }

    // --- Business Methods (Akt 2) ---

    /// <summary>Boards the missing crew from the pool, all or nobody.</summary>
    public void AssignCrew(BaseShip ship)
    {
        // Guard: ship must not be null
        ArgumentNullException.ThrowIfNull(ship);

        // Business Rules: docked here, pool has every missing role, enough seats
        Rules.Require(_ships.Contains(ship), $"{ship.Name} ist nicht an {Name} angedockt.");
        var selected = SelectMissingCrew(ship);
        Rules.Require(selected.Count == MissingCount(ship),
            $"Im Pool von {Name} fehlen Leute für {ship.Name}: {string.Join(", ", ship.MissingRoles())}.");
        Rules.Require(ship.CrewCount + selected.Count <= BaseShip.MaxCrew, $"{ship.Name} hat nicht genug Sitze.");

        // Business Logic: first check, then change
        foreach (var member in selected)
        {
            ship.Board(member);
            _crewPool.Remove(member);
        }

        // Audit Log
        Logbook.Add(LogType.Crew, $"{selected.Count} Personen an {ship.Name} zugeteilt");
    }

    /// <summary>Boards one chosen member from the pool onto a docked ship (Story).</summary>
    public void AssignMember(BaseShip ship, CrewMember member)
    {
        // Guard: ship and member must not be null
        ArgumentNullException.ThrowIfNull(ship);
        ArgumentNullException.ThrowIfNull(member);

        // Business Rules: docked here, member in the pool, a free seat
        Rules.Require(_ships.Contains(ship), $"{ship.Name} ist nicht an {Name} angedockt.");
        Rules.Require(_crewPool.Contains(member), $"{member.Name} ist nicht im Pool von {Name}.");
        Rules.Require(ship.CrewCount < BaseShip.MaxCrew, $"{ship.Name} hat keinen freien Sitz.");

        // Business Logic: on board, out of the pool
        ship.Board(member);
        _crewPool.Remove(member);

        // Audit Log
        Logbook.Add(LogType.Crew, $"{member} an {ship.Name} zugeteilt");
    }

    /// <summary>Takes the whole crew of a docked ship back into the pool.</summary>
    public void UnassignCrew(BaseShip ship)
    {
        // Guard: ship must not be null
        ArgumentNullException.ThrowIfNull(ship);

        // Business Rule: docked here
        Rules.Require(_ships.Contains(ship), $"{ship.Name} ist nicht an {Name} angedockt.");

        // Business Logic: every member back into the pool (copy, because Unboard changes Crew)
        var crew = ship.Crew.ToList();
        foreach (var member in crew)
        {
            ship.Unboard(member);
            _crewPool.Add(member);
        }

        // Audit Log
        Logbook.Add(LogType.Crew, $"{crew.Count} Personen von {ship.Name} zurück im Pool");
    }

    // --- Output ---

    /// <summary>Describes the station as a block of lines.</summary>
    public string Report() =>
        $"""
        {Name} (SpaceStation)
          {"Position",-12}{Position}
          {"Fuel Pool",-12}{FuelPool}
          {"Docks",-12}{ShipCount}/{MaxDocks}: {NamesOrDash(Ships.Select(s => s.Name))}
          {"Crew Pool",-12}{CrewCount}: {NamesOrDash(CrewPool.Select(c => c.ToString()))}
        """;

    // --- Helpers ---

    /// <summary>How many people the ship still needs for its required roles.</summary>
    private static int MissingCount(BaseShip ship) =>
        ship.RequiredRoles.Sum(r => Math.Max(0, r.Value - ship.Crew.Count(c => c.Role == r.Key)));

    /// <summary>Picks the missing members from the pool, in pool order.</summary>
    private List<CrewMember> SelectMissingCrew(BaseShip ship)
    {
        var selected = new List<CrewMember>();
        foreach (var member in _crewPool)
        {
            int required = ship.RequiredRoles.GetValueOrDefault(member.Role);
            int onBoard = ship.Crew.Count(c => c.Role == member.Role);
            int chosen = selected.Count(c => c.Role == member.Role);
            if (onBoard + chosen < required)
                selected.Add(member);
        }
        return selected;
    }

    private static string NamesOrDash(IEnumerable<string> names) =>
        names.Any() ? string.Join(", ", names) : "-";
}
