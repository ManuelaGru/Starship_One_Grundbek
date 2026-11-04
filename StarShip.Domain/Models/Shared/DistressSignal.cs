namespace StarShip.Domain.Models.Shared;

public record DistressSignal
{
    // --- Props ---

    public string Reason { get; init; }
    public DateTime SentAt { get; init; }

    // --- Ctor ---

    public DistressSignal(string reason, DateTime sentAt)
    {
        // Guard: reason must not be empty
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Reason = reason.Trim();
        SentAt = sentAt;
    }
}
