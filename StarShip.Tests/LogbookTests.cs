using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;

namespace StarShip.Tests;

public class LogbookTests
{
    private static readonly Coordinate Home = new(100, 200);

    // --- 4.1 Append ---

    [Fact]
    public void Append_ShouldKeepActor_WhenEntriesAreCopied()
    {
        // Given
        var rioGrande = new RescueShip("Rio Grande", Home);
        var xhosa = new CargoShip("Xhosa", Home, 100);
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // When
        rioGrande.Logbook.Append(xhosa.Logbook);

        // Then
        Assert.Equal(1, rioGrande.Logbook.Count);
        Assert.Equal("Xhosa", rioGrande.Logbook.Last?.Actor);
    }

    // --- 4.6 LastOf ---

    [Fact]
    public void LastOf_ShouldReturnNull_WhenTypeHasNoEntry()
    {
        // Given
        var xhosa = new CargoShip("Xhosa", Home, 100);
        xhosa.SendDistressSignal("Warpkern überhitzt");

        // When
        var rescue = xhosa.Logbook.LastOf(LogType.Rescue);

        // Then
        Assert.Null(rescue);
    }
}
