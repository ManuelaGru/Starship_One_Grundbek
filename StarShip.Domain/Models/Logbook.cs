using StarShip.Domain.Exceptions;
using StarShip.Domain.Models.Shared;
using StarShip.Domain.Queries;

namespace StarShip.Domain.Models;

public class Logbook
{
    // --- Constants ---

    private const string DistressLabel = "[!!]";

    // --- Fields ---

    private readonly List<LogEntry> _entries = [];     // Invariante: nur anhängen, nie löschen oder ändern

    // --- Props ---

    public BaseEntity Owner { get; }

    // --- Queries ---

    public IReadOnlyList<LogEntry> Entries => _entries.AsReadOnly();
    public int Count => _entries.Count;
    public LogEntry? First => _entries.FirstOrDefault();
    public LogEntry? Last => _entries.LastOrDefault();

    // --- Ctor ---

    public Logbook(BaseEntity owner)
    {
        // Guard: owner must not be null
        ArgumentNullException.ThrowIfNull(owner);

        Owner = owner;
    }

    // --- Business Methods ---

    /// <summary>Adds an entry with the current time and the owner as actor.</summary>
    public void Add(LogType type, string message)
    {
        // Guard: message must not be empty
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        // Business Logic: append only, never remove
        _entries.Add(new LogEntry(DateTime.UtcNow, type, Owner.Name, message.Trim()));
    }

    /// <summary>Appends all entries of another logbook, the actor stays the original one (Akt 4).</summary>
    public void Append(Logbook other)
    {
        // Guard: other must not be null
        ArgumentNullException.ThrowIfNull(other);

        // Business Rule: not the own logbook
        Rules.Require(other != this, $"Das Logbuch von {Owner.Name} kann sich nicht selbst anhängen.");

        // Business Logic: append a copy of all entries in order
        _entries.AddRange(other.Entries);
    }

    // --- Queries (Akt 4) ---

    // LINQ: Where, ToList
    /// <summary>All entries of one type in logbook order.</summary>
    public IReadOnlyList<LogEntry> ByType(LogType type) =>
        _entries.Where(e => e.Type == type).ToList();

    // LINQ: Where, LastOrDefault
    /// <summary>The last entry of one type, or null.</summary>
    public LogEntry? LastOf(LogType type) =>
        _entries.Where(e => e.Type == type).LastOrDefault();

    // LINQ: GroupBy, Select, OrderByDescending
    /// <summary>Number of entries per type, most frequent first.</summary>
    public IReadOnlyList<LogTypeCount> CountByType() =>
        _entries
            .GroupBy(e => e.Type)
            .Select(g => new LogTypeCount(g.Key, g.Count()))
            .OrderByDescending(c => c.Count)
            .ToList();

    // --- Output ---

    /// <summary>Formats one entry as three columns: label, actor, message (Akt 5).</summary>
    public string Format(LogEntry entry)
    {
        // Guard: entry must not be null
        ArgumentNullException.ThrowIfNull(entry);

        string label = entry.Type switch
        {
            LogType.Distress => DistressLabel,
            _                => $"[{entry.Type}]"
        };
        return $"{label,-10} {entry.Actor,-12} {entry.Message}";
    }

    // LINQ: Select
    /// <summary>All entries, one formatted line each (Akt 5).</summary>
    public string Report() =>
        string.Join(Environment.NewLine, _entries.Select(Format));

    /// <summary>Formats the logbook as Logbuch Owner, n Einträge (Akt 1).</summary>
    public override string ToString() => $"Logbuch {Owner.Name}, {Count} Einträge";
}
