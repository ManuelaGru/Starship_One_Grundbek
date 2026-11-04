using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;

namespace StarShip.Domain.Queries;

public static class ShipQueries
{
    // --- Akt 2 ---

    // LINQ: Where, ToList
    /// <summary>All ships that are in space without distress.</summary>
    public static IReadOnlyList<BaseShip> InSpace(this IEnumerable<BaseShip> ships) =>
        ships.Where(s => s.Status == ShipStatus.InSpace).ToList();

    // LINQ: Any
    /// <summary>True as soon as one ship is in space.</summary>
    public static bool AnyInSpace(this IEnumerable<BaseShip> ships) =>
        ships.Any(s => s.Status == ShipStatus.InSpace);

    // LINQ: All
    /// <summary>True if every ship is docked, also for an empty fleet.</summary>
    public static bool AllDocked(this IEnumerable<BaseShip> ships) =>
        ships.All(s => s.Status == ShipStatus.Docked);

    // LINQ: Where, Select, ToList
    /// <summary>The names of all ships with Readiness.Ready.</summary>
    public static IReadOnlyList<string> ReadyToLaunch(this IEnumerable<BaseShip> ships) =>
        ships
            .Where(s => s.Readiness == Readiness.Ready)
            .Select(s => s.Name)
            .ToList();

    // LINQ: OfType, Sum
    /// <summary>The sum of Cargo over all cargo ships.</summary>
    public static int TotalCargo(this IEnumerable<BaseShip> ships) =>
        ships.OfType<CargoShip>().Sum(c => c.Cargo);

    // LINQ: GroupBy, Select, ToList
    /// <summary>The sum of Fuel per type name.</summary>
    public static IReadOnlyList<FuelByType> TotalFuelByType(this IEnumerable<BaseShip> ships) =>
        ships
            .GroupBy(s => s.GetType().Name)
            .Select(g => new FuelByType(g.Key, g.Sum(s => s.Fuel)))
            .ToList();

    // --- Akt 3 ---

    // LINQ: Any
    /// <summary>True if at least one ship is in distress.</summary>
    public static bool AnyInDistress(this IEnumerable<BaseShip> ships) =>
        ships.Any(s => s.Status == ShipStatus.Distress);

    // LINQ: Where, ToList
    /// <summary>All ships in distress.</summary>
    public static IReadOnlyList<BaseShip> InDistress(this IEnumerable<BaseShip> ships) =>
        ships.Where(s => s.Status == ShipStatus.Distress).ToList();

    // LINQ: MinBy
    /// <summary>The ship nearest to the origin, or null.</summary>
    public static BaseShip? Nearest(this IEnumerable<BaseShip> ships, Coordinate origin) =>
        ships.MinBy(s => s.DistanceTo(origin));

    // LINQ: MaxBy
    /// <summary>The ship farthest from the origin, or null.</summary>
    public static BaseShip? Farthest(this IEnumerable<BaseShip> ships, Coordinate origin) =>
        ships.MaxBy(s => s.DistanceTo(origin));

    // LINQ: Where, OrderBy, Select, ToList
    /// <summary>One report per distress signal, oldest first.</summary>
    public static IReadOnlyList<DistressReport> DistressReports(this IEnumerable<BaseShip> ships) =>
        ships
            .Where(s => s.Distress is not null)
            .OrderBy(s => s.Distress?.SentAt)
            .Select(s => new DistressReport(s.Name, s.Distress!.Reason, s.Distress.SentAt))   // ! : Where hat null schon ausgefiltert
            .ToList();

    // --- Akt 4 ---

    // LINQ: Where, OrderBy, ToList
    /// <summary>All ships at most range light years from the origin, nearest first.</summary>
    public static IReadOnlyList<BaseShip> WithinRange(this IEnumerable<BaseShip> ships, Coordinate origin, int range) =>
        ships
            .Where(s => s.DistanceTo(origin) <= range)
            .OrderBy(s => s.DistanceTo(origin))
            .ToList();

    // --- Akt 5 ---

    // LINQ: MinBy
    /// <summary>The ship with the earliest BuiltAt, or null.</summary>
    public static BaseShip? Oldest(this IEnumerable<BaseShip> ships) =>
        ships.MinBy(s => s.BuiltAt);

    // LINQ: MaxBy
    /// <summary>The ship with the latest BuiltAt, or null.</summary>
    public static BaseShip? Newest(this IEnumerable<BaseShip> ships) =>
        ships.MaxBy(s => s.BuiltAt);

    // LINQ: MaxBy
    /// <summary>The ship with the largest CrewCount, or null.</summary>
    public static BaseShip? WithLargestCrew(this IEnumerable<BaseShip> ships) =>
        ships.MaxBy(s => s.CrewCount);

    // LINQ: GroupBy, Select, OrderBy
    /// <summary>Number of ships per build year, ascending.</summary>
    public static IReadOnlyList<YearCount> PerBuildYear(this IEnumerable<BaseShip> ships) =>
        ships
            .GroupBy(s => s.BuiltAt.Year)
            .Select(g => new YearCount(g.Key, g.Count()))
            .OrderBy(y => y.Year)
            .ToList();
}
