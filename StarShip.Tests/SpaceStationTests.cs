using StarShip.Domain.Exceptions;
using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;

namespace StarShip.Tests;

public class SpaceStationTests
{
    private static readonly Coordinate Home = new(100, 200);

    // --- 1.1 / 1.2 AddCrew ---

    [Fact]
    public void AddCrew_ShouldIncreaseCrewCount_WhenMemberIsNew()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);

        // When
        ds9.AddCrew(new CrewMember("Kira", CrewRole.Captain));

        // Then
        Assert.Equal(1, ds9.CrewCount);
        Assert.Equal(LogType.Crew, ds9.Logbook.Last?.Type);
    }

    [Fact]
    public void AddCrew_ShouldThrow_WhenMemberIsAlreadyInPool()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var kira = new CrewMember("Kira", CrewRole.Captain);
        ds9.AddCrew(kira);

        // When / Then
        Assert.Throws<StarshipException>(() => ds9.AddCrew(kira));
    }

    // --- 1.3 Supply ---

    [Fact]
    public void Supply_ShouldIncreaseFuelPool_WhenAmountIsPositive()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);

        // When
        ds9.Supply(40);

        // Then
        Assert.Equal(40, ds9.FuelPool);
        Assert.Equal(LogType.Fuel, ds9.Logbook.Last?.Type);
    }

    [Fact]
    public void Supply_ShouldThrow_WhenAmountIsZero()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);

        // When / Then
        Assert.Throws<ArgumentOutOfRangeException>(() => ds9.Supply(0));
    }

    // --- 1.5 Dock und Undock ---

    [Fact]
    public void Dock_ShouldSetBothSides_WhenShipIsInSpace()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);

        // When
        ds9.Dock(xhosa);

        // Then
        Assert.Contains(xhosa, ds9.Ships);
        Assert.Equal(ds9, xhosa.Station);
    }

    [Fact]
    public void Dock_ShouldThrow_WhenShipIsAlreadyDocked()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Dock(xhosa);

        // When / Then
        Assert.Throws<StarshipException>(() => ds9.Dock(xhosa));
    }

    [Fact]
    public void Dock_ShouldThrow_WhenShipSendsDistressSignal()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // When / Then
        Assert.Throws<StarshipException>(() => ds9.Dock(xhosa));
        Assert.Null(xhosa.Station);
    }

    // --- 1.8 Refuel ---

    [Fact]
    public void Refuel_ShouldFillWarpCore_WhenStationHasEnoughFuel()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Supply(40);
        ds9.Dock(xhosa);

        // When
        ds9.Refuel(xhosa);

        // Then
        Assert.Equal(BaseShip.MaxFuel, xhosa.Fuel);
        Assert.Equal(30, ds9.FuelPool);
    }

    [Fact]
    public void Refuel_ShouldThrow_WhenPoolIsTooSmall()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Supply(5);
        ds9.Dock(xhosa);

        // When / Then
        Assert.Throws<StarshipException>(() => ds9.Refuel(xhosa));
    }

    // --- 1.9 Queries ---

    [Fact]
    public void CrewCountByRole_ShouldReturnTwo_WhenTwoPilotsAreInPool()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        ds9.AddCrew(CrewMember.Pilot("Nog"));
        ds9.AddCrew(CrewMember.Pilot("Dax"));
        ds9.AddCrew(CrewMember.Captain("Kira"));

        // When
        int pilots = ds9.CrewCountByRole(CrewRole.Pilot);

        // Then
        Assert.Equal(2, pilots);
    }

    [Fact]
    public void CrewCountPerRole_ShouldReturnFourGroups_WhenPoolHasAllRoles()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        ds9.AddCrew(CrewMember.Medic("Bashir"));
        ds9.AddCrew(CrewMember.Captain("Kira"));
        ds9.AddCrew(CrewMember.Engineer("O'Brien"));
        ds9.AddCrew(CrewMember.Pilot("Nog"));

        // When
        var perRole = ds9.CrewCountPerRole();

        // Then
        Assert.Equal(4, perRole.Count);
        Assert.Equal(CrewRole.Captain, perRole[0].Role);
    }

    // --- 2.4 AssignCrew ---

    [Fact]
    public void AssignCrew_ShouldBoardCaptainAndPilot_WhenPoolHasBoth()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.AddCrew(CrewMember.Captain("Kira"));
        ds9.AddCrew(CrewMember.Pilot("Nog"));
        ds9.Dock(xhosa);

        // When
        ds9.AssignCrew(xhosa);

        // Then
        Assert.Equal(2, xhosa.CrewCount);
        Assert.True(xhosa.HasRequiredRoles);
        Assert.Equal(0, ds9.CrewCount);
    }

    [Fact]
    public void AssignCrew_ShouldThrow_WhenPoolHasNoPilot()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.AddCrew(CrewMember.Captain("Kira"));
        ds9.Dock(xhosa);

        // When / Then
        Assert.Throws<StarshipException>(() => ds9.AssignCrew(xhosa));
        Assert.Equal(0, xhosa.CrewCount);
        Assert.Equal(1, ds9.CrewCount);
    }

    // --- 5.2 AllShipsReady und AnyShipReady ---

    [Fact]
    public void AllShipsReady_ShouldBeFalse_WhenOneShipNeedsCrew()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        ds9.Supply(10);
        ds9.Dock(xhosa);
        ds9.Refuel(xhosa);

        // When
        bool allReady = ds9.AllShipsReady();

        // Then
        Assert.False(allReady);
    }

    [Fact]
    public void AnyShipReady_ShouldBeFalse_WhenDockIsEmpty()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);

        // When
        bool anyReady = ds9.AnyShipReady();

        // Then
        Assert.False(anyReady);
    }
}
