namespace StarShip.Domain.Exceptions;

public static class Rules
{
    // --- Business Methods ---

    /// <summary>Throws a StarshipException unless the business rule holds.</summary>
    public static void Require(bool condition, string message)
    {
        if (!condition) throw new StarshipException(message);
    }
}
