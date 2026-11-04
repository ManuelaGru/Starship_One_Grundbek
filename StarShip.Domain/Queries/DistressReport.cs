namespace StarShip.Domain.Queries;

/// <summary>One distress signal of the fleet.</summary>
public record DistressReport(string Name, string Reason, DateTime SentAt)
{
    public override string ToString() => $"{Name}: {Reason} ({SentAt:HH:mm:ss})";
}
