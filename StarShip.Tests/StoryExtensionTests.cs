using StarShip.Domain.Exceptions;
using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;

namespace StarShip.Tests;

/// <summary>Unit tests for the methods added for the own story.</summary>
public class StoryExtensionTests
{
    private static readonly Coordinate Home = new(100, 200);
    private static readonly Coordinate Far = new(100, 260);

    // --- ScoutShip ---

    [Fact]
    public void WarpTo_ShouldJumpScoutRange_WhenShipIsScout()
    {
        // Given
        var falke = new ScoutShip("Falke", Far);
        falke.Board(CrewMember.Captain("Vlax"));
        Fill(falke);

        // When
        falke.WarpTo(Home);

        // Then
        Assert.Equal(new Coordinate(100, 230), falke.Position);
    }

    [Fact]
    public void Ctor_ShouldRequireCaptain_WhenShipIsScout()
    {
        // When
        var falke = new ScoutShip("Falke", Far);

        // Then
        Assert.Equal([CrewRole.Captain], falke.MissingRoles());
    }

    // --- Transfer und TransferFuel ---

    [Fact]
    public void Transfer_ShouldThrow_WhenShipsAreNotConnected()
    {
        // Given
        var vlax = CrewMember.Captain("Vlax");
        var bokNor = new CargoShip("Bok'Nor", Far, 60);
        var falke = new ScoutShip("Falke", Home);
        bokNor.Board(vlax);

        // When / Then
        Assert.Throws<StarshipException>(() => bokNor.Transfer(vlax, falke));
        Assert.Contains(vlax, bokNor.Crew);
    }

    [Fact]
    public void TransferFuel_ShouldMoveAntimatter_WhenShipsAreConnected()
    {
        // Given
        var bokNor = new CargoShip("Bok'Nor", Far, 60);
        var falke = new ScoutShip("Falke", Far);
        Fill(bokNor);

        // When
        bokNor.TransferFuel(falke, 2);

        // Then
        Assert.Equal(BaseShip.MaxFuel - 2, bokNor.Fuel);
        Assert.Equal(2, falke.Fuel);
    }

    // --- Ersatz-Warpkern ---

    [Fact]
    public void HandOverSpareCore_ShouldThrow_WhenShipHasNoSpareCore()
    {
        // Given
        var bokNor = new CargoShip("Bok'Nor", Far, 60);
        var falke = new ScoutShip("Falke", Far);

        // When / Then
        Assert.Throws<StarshipException>(() => bokNor.HandOverSpareCore(falke));
    }

    [Fact]
    public void ReplaceCore_ShouldThrow_WhenNoEngineerIsOnBoard()
    {
        // Given
        var (_, xhosa) = BrokenXhosaWithSpareCore();
        xhosa.Unboard(CrewMember.Engineer("Serenade"));

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.ReplaceCore());
        Assert.Equal(CoreState.Overheated, xhosa.Core);
    }

    [Fact]
    public void ReleaseCore_ShouldThrow_WhenControlComputerIsOffline()
    {
        // Given
        var (_, xhosa) = BrokenXhosaWithSpareCore();
        xhosa.ReplaceCore();
        xhosa.FailControlComputer();

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.ReleaseCore());
        Assert.Equal(CoreState.AwaitingRelease, xhosa.Core);
    }

    [Fact]
    public void ReleaseCoreManually_ShouldBringCoreOnline_WhenEngineerIsOnBoard()
    {
        // Given
        var (_, xhosa) = BrokenXhosaWithSpareCore();
        xhosa.ReplaceCore();
        xhosa.FailControlComputer();

        // When
        xhosa.ReleaseCoreManually(CrewMember.Engineer("Serenade"));

        // Then
        Assert.Equal(CoreState.Online, xhosa.Core);
    }

    [Fact]
    public void WarpTo_ShouldThrow_WhenCoreIsOverheated()
    {
        // Given
        var (_, xhosa) = BrokenXhosaWithSpareCore();

        // When / Then
        Assert.Throws<StarshipException>(() => xhosa.WarpTo(Home));
        Assert.Equal(Readiness.NeedsRepair, xhosa.Readiness);
    }

    // --- Rettungsboote ---

    [Fact]
    public void CrewWithoutLifeboatSeat_ShouldReturnOne_WhenFiveAreOnBoard()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);
        foreach (var name in new[] { "A", "B", "C", "D", "E" })
            xhosa.Board(CrewMember.Engineer(name));

        // When
        int withoutSeat = xhosa.CrewWithoutLifeboatSeat;

        // Then
        Assert.Equal(1, withoutSeat);
    }

    // --- RescueShip: TakeInjured ---

    [Fact]
    public void TakeInjured_ShouldThrow_WhenNoMedicIsOnBoard()
    {
        // Given
        var nog = CrewMember.Pilot("Nog");
        var xhosa = new CargoShip("Xhosa", Home, 100);
        var rioGrande = new RescueShip("Rio Grande", Home);
        xhosa.Board(nog);
        rioGrande.Board(CrewMember.Captain("Worf"));
        rioGrande.Board(CrewMember.Pilot("Dax"));

        // When / Then
        Assert.Throws<StarshipException>(() => rioGrande.TakeInjured(xhosa, nog));
        Assert.Contains(nog, xhosa.Crew);
    }

    // --- SpaceStation: AssignMember ---

    [Fact]
    public void AssignMember_ShouldMoveMemberFromPoolToShip_WhenShipIsDocked()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var bokNor = new CargoShip("Bok'Nor", Home, 60);
        var toran = CrewMember.Captain("Toran");
        ds9.AddCrew(toran);
        ds9.Dock(bokNor);

        // When
        ds9.AssignMember(bokNor, toran);

        // Then
        Assert.Contains(toran, bokNor.Crew);
        Assert.Equal(0, ds9.CrewCount);
    }

    // --- BaseEntity: Radio ---

    [Fact]
    public void Radio_ShouldWriteBothLogbooks_WhenMessageIsSent()
    {
        // Given
        var bokNor = new CargoShip("Bok'Nor", Far, 60);
        var xhosa = new CargoShip("Xhosa", Home, 100);

        // When
        bokNor.Radio(xhosa, "Was ist passiert?");

        // Then
        Assert.Equal(LogType.Radio, bokNor.Logbook.Last?.Type);
        Assert.Equal("Funk von Bok'Nor: Was ist passiert?", xhosa.Logbook.Last?.Message);
    }

    // --- Helpers ---

    /// <summary>Fills a ship at a temporary station, then the ship is back in space at its position.</summary>
    private static void Fill(BaseShip ship)
    {
        var depot = new SpaceStation("Depot", ship.Position);
        depot.Supply(BaseShip.MaxFuel);
        depot.Dock(ship);
        depot.Refuel(ship);
        depot.Undock(ship);
    }

    /// <summary>A Xhosa in space with overheated core, Serenade on board and a spare core.</summary>
    private static (SpaceStation, CargoShip) BrokenXhosaWithSpareCore()
    {
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        xhosa.Board(CrewMember.Captain("Kira"));
        xhosa.Board(CrewMember.Pilot("Dax"));
        xhosa.Board(CrewMember.Engineer("Serenade"));
        ds9.Dock(xhosa);
        xhosa.LoadSpareCore();
        ds9.Undock(xhosa);
        xhosa.CoreFailure();
        return (ds9, xhosa);
    }
}
