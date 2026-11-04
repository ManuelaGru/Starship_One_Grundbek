using StarShip.Domain.Exceptions;
using StarShip.Domain.Models.Shared;
using StarShip.Domain.Queries;

namespace StarShip.Domain.Models;

public abstract class BaseShip : BaseEntity
{
    // --- Constants ---

    public const int MaxFuel = 10;
    public const int MaxCrew = 6;
    public const int WarpRange = 15;
    public const int SensorRange = 20;
    public const int FuelPerWarp = 1;
    public const int Lifeboats = 2;                        // Story: Rettungsboote je Schiff
    public const int SeatsPerLifeboat = 2;                 // Story: Plätze je Rettungsboot
    public const int MaxSpareCores = 1;                    // Story: höchstens ein Ersatz-Warpkern an Bord

    // --- Fields ---

    private readonly List<CrewMember> _crew = [];          // Invariante: höchstens MaxCrew, niemand doppelt

    // --- Props ---

    public IReadOnlyDictionary<CrewRole, int> RequiredRoles { get; }   // Invariante: mindestens eine Pflichtrolle, Summe höchstens MaxCrew
    public int Fuel { get; private set; }                  // Invariante: 0 bis MaxFuel
    public DistressSignal? Distress { get; private set; }  // Invariante: nur im All, nie angedockt
    public SpaceStation? Station { get; private set; }     // Invariante: gesetzt genau dann, wenn Station.Ships das Schiff enthält
    public int SpareCores { get; private set; }            // Story, Invariante: 0 bis MaxSpareCores
    public CoreState Core { get; private set; } = CoreState.Online;   // Story, Invariante: nur Online darf springen
    public bool ControlComputerOnline { get; private set; } = true;  // Story: ohne Kontrollcomputer keine normale Freigabe

    // --- Queries ---

    public IReadOnlyList<CrewMember> Crew => _crew.AsReadOnly();
    public int CrewCount => _crew.Count;
    public bool IsDocked => Station is not null;

    /// <summary>Light years per warp jump, a ship type may jump farther (Story).</summary>
    public virtual int JumpRange => WarpRange;

    public int LifeboatSeats => Lifeboats * SeatsPerLifeboat;
    public int CrewWithoutLifeboatSeat => Math.Max(0, CrewCount - LifeboatSeats);

    // LINQ: All, darin Count
    public bool HasRequiredRoles => RequiredRoles.All(r => CountOnBoard(r.Key) >= r.Value);

    public ShipStatus Status => this switch
    {
        { Distress: not null } => ShipStatus.Distress,
        { Station: null }      => ShipStatus.InSpace,
        _                      => ShipStatus.Docked
    };

    public Readiness Readiness => this switch
    {
        { Core: not CoreState.Online } => Readiness.NeedsRepair,
        { HasRequiredRoles: false } => Readiness.NeedsCrew,
        { Fuel: < MaxFuel }         => Readiness.NeedsFuel,
        _                           => Readiness.Ready
    };

    // LINQ: Where, Count, ToHashSet
    /// <summary>All required roles with fewer members on board than required.</summary>
    public IReadOnlySet<CrewRole> MissingRoles() =>
        RequiredRoles
            .Where(r => CountOnBoard(r.Key) < r.Value)
            .Select(r => r.Key)
            .ToHashSet();

    /// <summary>True if the antimatter lasts for the rounded up number of warp jumps to the target (Akt 2).</summary>
    public bool HasEnoughFuelTo(Coordinate target)
    {
        // Guard: target must not be null
        ArgumentNullException.ThrowIfNull(target);

        return Fuel >= JumpsTo(target) * FuelPerWarp;
    }

    /// <summary>Number of warp jumps from the own position to the target, rounded up (Story).</summary>
    public int JumpsTo(Coordinate target) => JumpsBetween(Position, target);

    /// <summary>Number of warp jumps this ship needs between two points, rounded up (Story).</summary>
    public int JumpsBetween(Coordinate from, Coordinate to)
    {
        // Guard: from and to must not be null
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        return (int)Math.Ceiling(from.DistanceTo(to) / JumpRange);
    }

    /// <summary>True if the ship is in space, has its required roles, a working core and enough antimatter (Akt 2).</summary>
    public bool CanWarpTo(Coordinate target) =>
        Status == ShipStatus.InSpace && HasRequiredRoles && Core == CoreState.Online && HasEnoughFuelTo(target);

    /// <summary>Same as CanWarpTo with the position of the station (Akt 2).</summary>
    public bool CanWarpTo(SpaceStation station)
    {
        // Guard: station must not be null
        ArgumentNullException.ThrowIfNull(station);

        return CanWarpTo(station.Position);
    }

    /// <summary>True if the other ship is at most SensorRange away (Akt 4).</summary>
    public bool IsInSensorRange(BaseShip other)
    {
        // Guard: other must not be null
        ArgumentNullException.ThrowIfNull(other);

        return DistanceTo(other) <= SensorRange;
    }

    // LINQ: Where, dann WithinRange
    /// <summary>All other ships within SensorRange, nearest first (Akt 4).</summary>
    public virtual IEnumerable<BaseShip> Scan(IEnumerable<BaseShip> ships)
    {
        // Guard: ships must not be null
        ArgumentNullException.ThrowIfNull(ships);

        return ships.Where(s => s != this).WithinRange(Position, SensorRange);
    }

    // --- Ctor ---

    protected BaseShip(string name, Coordinate position, IReadOnlyDictionary<CrewRole, int> requiredRoles)
        : base(name, position)
    {
        // Guard: roles not null, a captain is required, all required roles fit on board
        ArgumentNullException.ThrowIfNull(requiredRoles);
        ArgumentOutOfRangeException.ThrowIfLessThan(requiredRoles.GetValueOrDefault(CrewRole.Captain), 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(requiredRoles.Values.Sum(), MaxCrew);

        RequiredRoles = requiredRoles;
    }

    // --- Business Methods (Akt 1) ---

    /// <summary>Sets the station. Only SpaceStation.Dock calls it, after the station added the ship.</summary>
    public void AttachTo(SpaceStation station)
    {
        // Guard: station must not be null
        ArgumentNullException.ThrowIfNull(station);

        // Business Rules: not docked yet, the station already lists the ship
        Rules.Require(!IsDocked, $"{Name} ist schon an {Station?.Name} angedockt.");
        Rules.Require(station.Ships.Contains(this), $"{station.Name} führt {Name} nicht in der Liste, andocken nur über Dock.");

        // Business Logic: set the child side
        Station = station;

        // Audit Log
        Logbook.Add(LogType.Docking, $"An {station.Name} angedockt");
    }

    /// <summary>Clears the station. Only SpaceStation.Undock calls it, after the station removed the ship.</summary>
    public void Detach()
    {
        // Business Rules: docked, the station no longer lists the ship
        var station = Station ?? throw new StarshipException($"{Name} ist nicht angedockt.");
        Rules.Require(!station.Ships.Contains(this), $"{station.Name} führt {Name} noch in der Liste, abdocken nur über Undock.");

        // Business Logic: clear the child side
        Station = null;

        // Audit Log
        Logbook.Add(LogType.Docking, $"Von {station.Name} abgedockt");
    }

    /// <summary>Puts antimatter into the warp core, only docked and never above MaxFuel.</summary>
    public void AddFuel(int amount)
    {
        // Guard: amount must be positive
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        // Business Rules: docked, warp core does not overflow
        Rules.Require(IsDocked, $"{Name} tankt nur im Dock.");
        Rules.Require(Fuel + amount <= MaxFuel, $"{Name} fasst nur {MaxFuel} Antimaterie, geladen {Fuel}.");

        // Business Logic: fill the warp core
        Fuel += amount;

        // Audit Log
        Logbook.Add(LogType.Fuel, $"{amount} Antimaterie geladen, Antimaterie {Fuel}/{MaxFuel}");
    }

    // --- Business Methods (Akt 2) ---

    /// <summary>Takes a crew member on board.</summary>
    public void Board(CrewMember member)
    {
        // Guard: member must not be null
        ArgumentNullException.ThrowIfNull(member);

        // Business Rules: a free seat, not on board yet
        Rules.Require(CrewCount < MaxCrew, $"{Name} hat nur {MaxCrew} Sitze.");
        Rules.Require(!_crew.Contains(member), $"{member.Name} ist schon an Bord von {Name}.");

        // Business Logic: add to the crew
        _crew.Add(member);

        // Audit Log
        Logbook.Add(LogType.Crew, $"{member} an Bord");
    }

    /// <summary>Lets a crew member leave the ship.</summary>
    public void Unboard(CrewMember member)
    {
        // Guard: member must not be null
        ArgumentNullException.ThrowIfNull(member);

        // Business Rule: on board
        Rules.Require(_crew.Contains(member), $"{member.Name} ist nicht an Bord von {Name}.");

        // Business Logic: remove from the crew
        _crew.Remove(member);

        // Audit Log
        Logbook.Add(LogType.Crew, $"{member} von Bord");
    }

    /// <summary>Checks the launch conditions and lets the station undock the ship.</summary>
    public void Launch()
    {
        // Business Rules: docked, required roles on board, warp core full
        var station = Station ?? throw new StarshipException($"{Name} ist nicht angedockt.");
        Rules.Require(HasRequiredRoles, $"{Name} fehlt Crew: {string.Join(", ", MissingRoles())}.");
        Rules.Require(Fuel == MaxFuel, $"{Name} hat nur {Fuel}/{MaxFuel} Antimaterie.");

        // Business Logic: the station releases the ship
        station.Undock(this);
    }

    /// <summary>Jumps at most WarpRange towards the target and burns FuelPerWarp.</summary>
    public void WarpTo(Coordinate target)
    {
        // Guard: target must not be null
        ArgumentNullException.ThrowIfNull(target);

        // Business Rules: in space, required roles on board, enough antimatter, not already there
        Rules.Require(Status == ShipStatus.InSpace, $"{Name} ist nicht frei im All, Status {Status}.");
        Rules.Require(HasRequiredRoles, $"{Name} fehlt Crew: {string.Join(", ", MissingRoles())}.");
        Rules.Require(Core == CoreState.Online, $"Der Warpkern von {Name} ist nicht freigegeben, Zustand {Core}.");
        Rules.Require(HasEnoughFuelTo(target), $"Die Antimaterie von {Name} reicht nicht bis {target}.");
        Rules.Require(Position != target, $"{Name} ist schon bei {target}.");

        // Business Logic: move, then burn antimatter
        MoveTo(NextPosition(target));
        Fuel -= FuelPerWarp;

        // Audit Log
        Logbook.Add(LogType.Warp, $"Warp-Sprung nach {Position}, Antimaterie {Fuel}/{MaxFuel}");
    }

    /// <summary>Jumps towards the station and docks on arrival.</summary>
    public void WarpTo(SpaceStation station)
    {
        // Guard: station must not be null
        ArgumentNullException.ThrowIfNull(station);

        // Business Logic: jump, dock when arrived
        WarpTo(station.Position);
        if (Position == station.Position)
            station.Dock(this);
    }

    // --- Business Methods (Akt 3) ---

    /// <summary>Sends a distress signal, only in space and only once.</summary>
    public void SendDistressSignal(string reason)
    {
        // Guard: reason must not be empty
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        // Business Rule: only a ship in space without a signal
        Rules.Require(Status == ShipStatus.InSpace, $"{Name} kann nur frei im All einen Notruf senden, Status {Status}.");

        // Business Logic: set the signal
        var signal = new DistressSignal(reason, DateTime.UtcNow);
        Distress = signal;

        // Audit Log
        Logbook.Add(LogType.Distress, $"NOTRUF: {signal.Reason}");
    }

    /// <summary>Clears the distress signal.</summary>
    public void ClearDistressSignal()
    {
        // Business Rule: there is a signal
        Rules.Require(Distress is not null, $"{Name} hat keinen Notruf.");

        // Business Logic: clear the signal
        Distress = null;

        // Audit Log
        Logbook.Add(LogType.Distress, "Notruf aufgehoben");
    }

    // --- Business Methods (Story) ---

    /// <summary>Moves a crew member to another ship at the same position.</summary>
    public void Transfer(CrewMember member, BaseShip target)
    {
        // Guard: member and target must not be null
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(target);

        // Business Rules: another ship, same position, on board here, a free seat there (all before the first change)
        Rules.Require(target != this, $"{member.Name} ist schon auf {Name}.");
        Rules.Require(Position == target.Position, $"{Name} und {target.Name} sind nicht verbunden.");
        Rules.Require(_crew.Contains(member), $"{member.Name} ist nicht an Bord von {Name}.");
        Rules.Require(target.CrewCount < MaxCrew, $"{target.Name} hat keinen freien Sitz.");

        // Business Logic: leave here, board there (both log)
        Unboard(member);
        target.Board(member);
    }

    /// <summary>Gives antimatter to another ship at the same position.</summary>
    public void TransferFuel(BaseShip target, int amount)
    {
        // Guard: target not null, amount positive
        ArgumentNullException.ThrowIfNull(target);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        // Business Rules: another ship, same position, enough here, no overflow there
        Rules.Require(target != this, $"{Name} kann sich nicht selbst betanken.");
        Rules.Require(Position == target.Position, $"{Name} und {target.Name} sind nicht verbunden.");
        Rules.Require(Fuel >= amount, $"{Name} hat nur {Fuel} Antimaterie.");
        Rules.Require(target.Fuel + amount <= MaxFuel, $"{target.Name} fasst nur {MaxFuel} Antimaterie, geladen {target.Fuel}.");

        // Business Logic: from here to there
        Fuel -= amount;
        target.Fuel += amount;

        // Audit Log: both ships
        Logbook.Add(LogType.Fuel, $"{amount} Antimaterie an {target.Name} abgegeben, Antimaterie {Fuel}/{MaxFuel}");
        target.Logbook.Add(LogType.Fuel, $"{amount} Antimaterie von {Name} erhalten, Antimaterie {target.Fuel}/{MaxFuel}");
    }

    /// <summary>Loads a spare warp core, only docked.</summary>
    public void LoadSpareCore()
    {
        // Business Rules: docked, room for a spare core
        Rules.Require(IsDocked, $"{Name} lädt nur im Dock.");
        Rules.Require(SpareCores < MaxSpareCores, $"{Name} hat schon einen Ersatz-Warpkern.");

        // Business Logic: one more spare core
        SpareCores++;

        // Audit Log
        Logbook.Add(LogType.Repair, "Ersatz-Warpkern an Bord");
    }

    /// <summary>Hands the spare warp core to another ship at the same position.</summary>
    public void HandOverSpareCore(BaseShip target)
    {
        // Guard: target must not be null
        ArgumentNullException.ThrowIfNull(target);

        // Business Rules: another ship, same position, a core here, room there
        Rules.Require(target != this, $"{Name} kann den Kern nicht an sich selbst übergeben.");
        Rules.Require(Position == target.Position, $"{Name} und {target.Name} sind nicht verbunden.");
        Rules.Require(SpareCores > 0, $"{Name} hat keinen Ersatz-Warpkern.");
        Rules.Require(target.SpareCores < MaxSpareCores, $"{target.Name} hat schon einen Ersatz-Warpkern.");

        // Business Logic: from here to there
        SpareCores--;
        target.SpareCores++;

        // Audit Log: both ships
        Logbook.Add(LogType.Repair, $"Ersatz-Warpkern an {target.Name} übergeben");
        target.Logbook.Add(LogType.Repair, $"Ersatz-Warpkern von {Name} übernommen");
    }

    /// <summary>The warp core overheats and shuts down.</summary>
    public void CoreFailure()
    {
        // Business Rule: only a working core can fail
        Rules.Require(Core == CoreState.Online, $"Der Warpkern von {Name} ist schon ausgefallen.");

        // Business Logic: the core is overheated
        Core = CoreState.Overheated;

        // Audit Log
        Logbook.Add(LogType.Repair, "Warpkern überhitzt und abgeschaltet");
    }

    /// <summary>Replaces the overheated core with the spare core, an engineer is needed.</summary>
    public void ReplaceCore()
    {
        // Business Rules: core overheated, spare core on board, an engineer on board
        Rules.Require(Core == CoreState.Overheated, $"Der Warpkern von {Name} ist nicht defekt.");
        Rules.Require(SpareCores > 0, $"{Name} hat keinen Ersatz-Warpkern.");
        Rules.Require(CountOnBoard(CrewRole.Engineer) > 0, $"Auf {Name} ist kein Engineer für den Einbau.");

        // Business Logic: the spare core is installed and waits for the safety release
        SpareCores--;
        Core = CoreState.AwaitingRelease;

        // Audit Log
        Logbook.Add(LogType.Repair, "Ersatz-Warpkern eingebaut, wartet auf Sicherheitsfreigabe");
    }

    /// <summary>The control computer that checks warp cores breaks down.</summary>
    public void FailControlComputer()
    {
        // Business Rule: only a working computer can fail
        Rules.Require(ControlComputerOnline, $"Der Kontrollcomputer von {Name} ist schon ausgefallen.");

        // Business Logic: computer offline
        ControlComputerOnline = false;

        // Audit Log
        Logbook.Add(LogType.Repair, "Kontrollcomputer ausgefallen");
    }

    /// <summary>Safety release of the new core by the control computer.</summary>
    public void ReleaseCore()
    {
        // Business Rules: core waits for release, control computer works
        Rules.Require(Core == CoreState.AwaitingRelease, $"Der Warpkern von {Name} wartet auf keine Freigabe.");
        Rules.Require(ControlComputerOnline, $"Kontrollcomputer von {Name} ausgefallen, keine Sicherheitsfreigabe möglich.");

        // Business Logic: core online
        Core = CoreState.Online;

        // Audit Log
        Logbook.Add(LogType.Repair, "Sicherheitsfreigabe durch den Kontrollcomputer");
    }

    /// <summary>Safety release with an improvised check by an engineer on board.</summary>
    public void ReleaseCoreManually(CrewMember engineer)
    {
        // Guard: engineer must not be null
        ArgumentNullException.ThrowIfNull(engineer);

        // Business Rules: core waits for release, engineer on board, role Engineer
        Rules.Require(Core == CoreState.AwaitingRelease, $"Der Warpkern von {Name} wartet auf keine Freigabe.");
        Rules.Require(_crew.Contains(engineer), $"{engineer.Name} ist nicht an Bord von {Name}.");
        Rules.Require(engineer.Role == CrewRole.Engineer, $"{engineer} ist kein Engineer.");

        // Business Logic: core online
        Core = CoreState.Online;

        // Audit Log
        Logbook.Add(LogType.Repair, $"Sicherheitsfreigabe per Behelfsprüfung von {engineer}");
    }

    // --- Output ---

    /// <summary>Formats the ship as Name, Status, bei Position.</summary>
    public override string ToString() => $"{Name}, {Status}, bei {Position}";

    /// <summary>Describes the ship as a block of lines.</summary>
    public virtual string Report() =>
        $"""
        {Name} ({GetType().Name})
          {"Status",-12}{Status}
          {"Readiness",-12}{Readiness}
          {"Position",-12}{Position}
          {"Fuel",-12}{Fuel}/{MaxFuel}
          {"Crew",-12}{CrewCount}/{MaxCrew}: {NamesOrDash(Crew.Select(c => c.ToString()))}
        """;

    // --- Helpers ---

    private int CountOnBoard(CrewRole role) => _crew.Count(c => c.Role == role);

    private Coordinate NextPosition(Coordinate target)
    {
        double distance = DistanceTo(target);
        return distance <= JumpRange ? target : StepToward(target, JumpRange / distance);
    }

    private Coordinate StepToward(Coordinate target, double factor) =>
        new((int)Math.Round(Position.X + (target.X - Position.X) * factor),
            (int)Math.Round(Position.Y + (target.Y - Position.Y) * factor));

    private static string NamesOrDash(IEnumerable<string> names) =>
        names.Any() ? string.Join(", ", names) : "-";
}
