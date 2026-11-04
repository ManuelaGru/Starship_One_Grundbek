using StarShip.Domain.Exceptions;
using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;

namespace StarShip.Tests;

public class CargoShipTests
{
    private static readonly Coordinate Home = new(100, 200);

    // --- 2.5 Load ---

    [Fact]
    public void Load_ShouldThrow_WhenCargoWouldExceedMaxCargo()
    {
        // Given
        var ds9 = new SpaceStation("Deep Space 9", Home);
        var bokNor = new CargoShip("Bok'Nor", Home, 60);
        ds9.Dock(bokNor);
        bokNor.Load(60);

        // When / Then
        Assert.Throws<StarshipException>(() => bokNor.Load(1));
        Assert.Equal(60, bokNor.Cargo);
    }
}
