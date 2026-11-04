namespace StarShip.Domain.Models.Shared;

public record LogEntry(DateTime At, LogType Type, string Actor, string Message);
