using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;
using StarShip.Domain.Queries;

namespace StarShip.Tests;

public class ShipQueriesTests
{
    private static readonly Coordinate Home = new(100, 200);
    private static readonly Coordinate Target = new(145, 200);

    // --- 2.10 ---

    [Fact]
    public void ReadyToLaunch_ShouldReturnXhosa_WhenOnlyXhosaIsReady()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        var bokNor = new CargoShip("Bok'Nor", Home, 60);
        ds9.Supply(20);
        ds9.AddCrew(CrewMember.Captain("Kira"));
        ds9.AddCrew(CrewMember.Pilot("Nog"));
        ds9.Dock(xhosa);
        ds9.Dock(bokNor);
        ds9.Refuel(xhosa);
        ds9.Refuel(bokNor);
        ds9.AssignCrew(xhosa);

        // When
        var ready = new List<BaseShip> { xhosa, bokNor }.ReadyToLaunch();

        // Then
        Assert.Equal(["Xhosa"], ready);
    }

    [Fact]
    public void AllDocked_ShouldBeTrue_WhenFleetIsEmpty()
    {
        // Given
        List<BaseShip> fleet = [];

        // When
        bool allDocked = fleet.AllDocked();

        // Then
        Assert.True(allDocked);
    }

    [Fact]
    public void TotalFuelByType_ShouldReturnEmpty_WhenFleetIsEmpty()
    {
        // Given
        List<BaseShip> fleet = [];

        // When
        var fuel = fleet.TotalFuelByType();

        // Then
        Assert.Empty(fuel);
    }

    // --- 3.2 ---

    [Fact]
    public void AnyInDistress_ShouldBeTrue_WhenOneShipSentASignal()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);
        var bokNor = new CargoShip("Bok'Nor", Home, 60);
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // When
        bool any = new List<BaseShip> { xhosa, bokNor }.AnyInDistress();

        // Then
        Assert.True(any);
    }

    [Fact]
    public void Nearest_ShouldReturnNull_WhenFleetIsEmpty()
    {
        // Given
        List<BaseShip> fleet = [];

        // When
        var nearest = fleet.Nearest(Home);

        // Then
        Assert.Null(nearest);
    }

    [Fact]
    public void Farthest_ShouldReturnXhosa_WhenXhosaIsAtTarget()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Target, 100);
        var bokNor = new CargoShip("Bok'Nor", Home, 60);

        // When
        var farthest = new List<BaseShip> { xhosa, bokNor }.Farthest(Home);

        // Then
        Assert.Equal(xhosa, farthest);
    }

    // --- 4.3 ---

    [Fact]
    public void WithinRange_ShouldIncludeShip_WhenDistanceEqualsRange()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", new Coordinate(100 + BaseShip.SensorRange, 200), 100);

        // When
        var inRange = new List<BaseShip> { xhosa }.WithinRange(Home, BaseShip.SensorRange);

        // Then
        Assert.Contains(xhosa, inRange);
    }

    // --- 5.1 ---

    [Fact]
    public void Oldest_ShouldReturnNull_WhenFleetIsEmpty()
    {
        // Given
        List<BaseShip> fleet = [];

        // When
        var oldest = fleet.Oldest();

        // Then
        Assert.Null(oldest);
    }

    [Fact]
    public void PerBuildYear_ShouldReturnThreeYears_WhenAllShipsDiffer()
    {
        // Given
        List<BaseShip> fleet =
        [
            new CargoShip("Xhosa", Home, 100) { BuiltAt = new DateOnly(2360, 5, 12) },
            new CargoShip("Bok'Nor", Home, 60) { BuiltAt = new DateOnly(2355, 7, 1) },
            new RescueShip("Rio Grande", Home) { BuiltAt = new DateOnly(2369, 11, 20) }
        ];

        // When
        var perYear = fleet.PerBuildYear();

        // Then
        Assert.Equal(3, perYear.Count);
        Assert.Equal(2355, perYear[0].Year);
    }
}
