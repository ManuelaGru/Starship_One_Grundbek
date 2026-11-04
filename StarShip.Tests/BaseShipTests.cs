using StarShip.Domain.Exceptions;
using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;

namespace StarShip.Tests;

public class BaseShipTests
{
    private static readonly Coordinate Home = new(100, 200);
    private static readonly Coordinate Target = new(145, 200);

    // --- 1.6 AttachTo ---

    [Fact]
    public void AttachTo_ShouldThrow_WhenStationDoesNotListTheShip()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.AttachTo(ds9));
        Assert.Null(xhosa.Station);
    }

    // --- 1.7 AddFuel ---

    [Fact]
    public void AddFuel_ShouldThrow_WhenWarpCoreWouldOverflow()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Dock(xhosa);
        xhosa.AddFuel(BaseShip.MaxFuel);

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.AddFuel(1));
    }

    // --- 1.10 Queries ---

    [Fact]
    public void Status_ShouldBeInSpace_WhenShipHasNoStation()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);

        // When
        var status = xhosa.Status;

        // Then
        Assert.Equal(ShipStatus.InSpace, status);
    }

    [Fact]
    public void Readiness_ShouldBeNeedsCrew_WhenCaptainIsMissing()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);
        xhosa.Board(CrewMember.Pilot("Nog"));

        // When
        var readiness = xhosa.Readiness;

        // Then
        Assert.Equal(Readiness.NeedsCrew, readiness);
    }

    [Fact]
    public void MissingRoles_ShouldReturnPilot_WhenOnlyCaptainIsOnBoard()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);
        xhosa.Board(CrewMember.Captain("Kira"));

        // When
        var missing = xhosa.MissingRoles();

        // Then
        Assert.Equal([CrewRole.Pilot], missing);
    }

    // --- 2.3 Board ---

    [Fact]
    public void Board_ShouldThrow_WhenShipIsFull()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);
        for (int i = 1; i <= BaseShip.MaxCrew; i++)
            xhosa.Board(CrewMember.Engineer($"Ingenieur {i}"));

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.Board(CrewMember.Captain("Kira")));
    }

    // --- 2.6 Launch ---

    [Fact]
    public void Launch_ShouldUndock_WhenShipIsReady()
    {
        // Given
        var (ds9, xhosa) = ReadyXhosa();

        // When
        xhosa.Launch();

        // Then
        Assert.Null(xhosa.Station);
        Assert.DoesNotContain(xhosa, ds9.Ships);
    }

    [Fact]
    public void Launch_ShouldThrow_WhenCaptainIsMissing()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Supply(10);
        ds9.Dock(xhosa);
        ds9.Refuel(xhosa);
        xhosa.Board(CrewMember.Pilot("Nog"));

        // When
        var ex = Assert.Throws<StarshipException>(() => xhosa.Launch());

        // Then
        Assert.Contains("Captain", ex.Message);
    }

    // --- 2.8 WarpTo ---

    [Fact]
    public void WarpTo_ShouldMoveWarpRangeAndBurnOneFuel_WhenTargetIsFar()
    {
        // Given
        var (_, xhosa) = ReadyXhosa();
        xhosa.Launch();

        // When
        xhosa.WarpTo(Target);

        // Then
        Assert.Equal(new Coordinate(115, 200), xhosa.Position);
        Assert.Equal(BaseShip.MaxFuel - BaseShip.FuelPerWarp, xhosa.Fuel);
    }

    [Fact]
    public void WarpTo_ShouldThrow_WhenFuelDoesNotReachTarget()
    {
        // Given
        var (_, xhosa) = ReadyXhosa();
        xhosa.Launch();
        var farAway = new Coordinate(100 + BaseShip.WarpRange * (BaseShip.MaxFuel + 1), 200);

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.WarpTo(farAway));
        Assert.Equal(Home, xhosa.Position);
    }

    // --- 3.1 SendDistressSignal ---

    [Fact]
    public void SendDistressSignal_ShouldSetStatusDistress_WhenShipIsInSpace()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);

        // When
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // Then
        Assert.Equal(ShipStatus.Distress, xhosa.Status);
        Assert.Equal("Warpkern überhitzt", xhosa.Distress?.Reason);
    }

    [Fact]
    public void SendDistressSignal_ShouldThrow_WhenShipIsDocked()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Dock(xhosa);

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.SendDistressSignal("Warpkern überhitzt"));
    }

    // --- Helpers ---

    /// <summary>A docked Xhosa with captain, pilot and a full warp core.</summary>
    private static (SpaceStation, CargoShip) ReadyXhosa()
    {
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Supply(10);
        ds9.AddCrew(CrewMember.Captain("Kira"));
        ds9.AddCrew(CrewMember.Pilot("Nog"));
        ds9.Dock(xhosa);
        ds9.Refuel(xhosa);
        ds9.AssignCrew(xhosa);
        return (ds9, xhosa);
    }
}
