using StarShip.Domain.Exceptions;
using StarShip.Domain.Models.Shared;

namespace StarShip.Domain.Models;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    // --- Props ---

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateOnly BuiltAt { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public Logbook Logbook { get; }
    public string Name { get; init; }
    public Coordinate Position { get; private set; }

    // --- Ctor ---

    protected BaseEntity(string name, Coordinate position)
    {
        // Guard: name must not be empty, position must not be null
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(position);

        Name = name.Trim();
        Position = position;
        Logbook = new Logbook(this);
    }

    // --- Business Methods ---

    protected void MoveTo(Coordinate target)
    {
        // Guard: target must not be null
        ArgumentNullException.ThrowIfNull(target);

        // Business Logic: change the position
        Position = target;
    }

    // --- Business Methods (Story) ---

    /// <summary>Sends a radio message, both logbooks record it.</summary>
    public void Radio(BaseEntity receiver, string message)
    {
        // Guard: receiver must not be null, message must not be empty
        ArgumentNullException.ThrowIfNull(receiver);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        // Business Rule: nobody radios himself
        Rules.Require(receiver != this, $"{Name} kann nicht sich selbst anfunken.");

        // Audit Log: sender and receiver
        Logbook.Add(LogType.Radio, $"Funk an {receiver.Name}: {message.Trim()}");
        receiver.Logbook.Add(LogType.Radio, $"Funk von {Name}: {message.Trim()}");
    }

    // --- Queries (Akt 2) ---

    /// <summary>Distance from this entity to a point in light years.</summary>
    public double DistanceTo(Coordinate target)
    {
        // Guard: target must not be null
        ArgumentNullException.ThrowIfNull(target);

        return Position.DistanceTo(target);
    }

    /// <summary>Distance from this entity to another entity in light years.</summary>
    public double DistanceTo(BaseEntity other)
    {
        // Guard: other must not be null
        ArgumentNullException.ThrowIfNull(other);

        return Position.DistanceTo(other.Position);
    }

    // --- Equality ---

    public bool Equals(BaseEntity? other) =>
        other is not null &&
        other.GetType() == GetType() &&
        other.Id == Id;

    public override bool Equals(object? obj) =>
        Equals(obj as BaseEntity);

    public override int GetHashCode() =>
        Id.GetHashCode();

    // --- Equality Operators ---

    public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(BaseEntity? left, BaseEntity? right) =>
        !(left == right);

    // --- Output (Akt 1) ---

    /// <summary>Formats the entity as Name bei Position, e.g. Deep Space 9 bei (100,200) Lichtjahre.</summary>
    public override string ToString() => $"{Name} bei {Position}";
}
