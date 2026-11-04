using StarShip.Domain.Exceptions;
using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;

namespace StarShip.Tests;

public class RescueShipTests
{
    private static readonly Coordinate Home = new(100, 200);

    // --- 4.2 Rescue ---

    [Fact]
    public void Rescue_ShouldTakeOverCrewAndClearSignal_WhenAllRulesHold()
    {
        // Given
        var (ds9, rioGrande) = ReadyRioGrande();
        rioGrande.Launch();
        var xhosa = new CargoShip("Xhosa", Home, 100);
        xhosa.Board(CrewMember.Captain("Kira"));
        xhosa.Board(CrewMember.Pilot("Nog"));
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // When
        rioGrande.Rescue(xhosa, ds9);

        // Then
        Assert.Equal(5, rioGrande.CrewCount);
        Assert.Equal(0, xhosa.CrewCount);
        Assert.Null(xhosa.Distress);
        Assert.Equal(LogType.Rescue, rioGrande.Logbook.Last?.Type);
    }

    [Fact]
    public void Rescue_ShouldThrow_WhenShipsAreNotAtTheSamePosition()
    {
        // Given
        var (ds9, rioGrande) = ReadyRioGrande();
        rioGrande.Launch();
        var xhosa = new CargoShip("Xhosa", new Coordinate(145, 200), 100);
        xhosa.Board(CrewMember.Captain("Kira"));
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // When / Then
        Assert.False(rioGrande.CanRescue(xhosa, ds9));
        Assert.Throws<StarshipException>(() => rioGrande.Rescue(xhosa, ds9));
        Assert.Equal(3, rioGrande.CrewCount);
        Assert.Equal(1, xhosa.CrewCount);
    }

    // --- 4.5 Scan ---

    [Fact]
    public void Scan_ShouldReturnOnlyShipsInDistress_WhenShipIsRescueShip()
    {
        // Given
        var rioGrande = new RescueShip("Rio Grande", Home);
        var xhosa = new CargoShip("Xhosa", new Coordinate(110, 200), 100);
        var bokNor = new CargoShip("Bok'Nor", new Coordinate(105, 200), 60);
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // When
        var hits = rioGrande.Scan([rioGrande, xhosa, bokNor]).ToList();

        // Then
        Assert.Equal([xhosa], hits);
    }

    // --- Helpers ---

    /// <summary>A docked Rio Grande with captain, pilot, medic and a full warp core.</summary>
    private static (SpaceStation, RescueShip) ReadyRioGrande()
    {
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var rioGrande = new RescueShip("Rio Grande", Home);
        ds9.Supply(10);
        ds9.AddCrew(CrewMember.Captain("Worf"));
        ds9.AddCrew(CrewMember.Pilot("Dax"));
        ds9.AddCrew(CrewMember.Medic("Bashir"));
        ds9.Dock(rioGrande);
        ds9.Refuel(rioGrande);
        ds9.AssignCrew(rioGrande);
        return (ds9, rioGrande);
    }
}
