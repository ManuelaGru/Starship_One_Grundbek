using StarShip.Domain.Exceptions;
using StarShip.Domain.Models.Shared;

namespace StarShip.Domain.Models;

public class CargoShip : BaseShip
{
    // --- Constants ---

    private static readonly IReadOnlyDictionary<CrewRole, int> Roles =
        new Dictionary<CrewRole, int> { [CrewRole.Captain] = 1, [CrewRole.Pilot] = 1 };

    // --- Props ---

    public int MaxCargo { get; init; }       // Invariante: größer als 0
    public int Cargo { get; private set; }   // Invariante: 0 bis MaxCargo

    // --- Queries (Akt 2) ---

    public bool IsFull => Cargo == MaxCargo;
    public int CargoPercent => Cargo * 100 / MaxCargo;

    // --- Ctor ---

    public CargoShip(string name, Coordinate position, int maxCargo) : base(name, position, Roles)
    {
        // Guard: maxCargo must be positive
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCargo);

        MaxCargo = maxCargo;
    }

    // --- Business Methods (Akt 2) ---

    /// <summary>Loads cargo in tons, only docked and never above MaxCargo.</summary>
    public void Load(int tons)
    {
        // Guard: tons must be positive
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tons);

        // Business Rules: docked, cargo does not exceed MaxCargo
        Rules.Require(IsDocked, $"{Name} lädt nur im Dock.");
        Rules.Require(Cargo + tons <= MaxCargo, $"{Name} fasst nur {MaxCargo} t, geladen {Cargo} t.");

        // Business Logic: increase the cargo
        Cargo += tons;

        // Audit Log
        Logbook.Add(LogType.Cargo, $"{tons} t geladen, {Cargo}/{MaxCargo}");
    }

    /// <summary>Unloads cargo in tons, only docked and never more than loaded.</summary>
    public void Unload(int tons)
    {
        // Guard: tons must be positive
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tons);

        // Business Rules: docked, never more than loaded
        Rules.Require(IsDocked, $"{Name} entlädt nur im Dock.");
        Rules.Require(tons <= Cargo, $"{Name} hat nur {Cargo} t geladen.");

        // Business Logic: decrease the cargo
        Cargo -= tons;

        // Audit Log
        Logbook.Add(LogType.Cargo, $"{tons} t entladen, {Cargo}/{MaxCargo}");
    }

    // --- Output ---

    /// <summary>The ship report plus one line Cargo.</summary>
    public override string Report() =>
        base.Report() + Environment.NewLine + $"  {"Cargo",-12}{Cargo}/{MaxCargo} t ({CargoPercent} %)";
}
