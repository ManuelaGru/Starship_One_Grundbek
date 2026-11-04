using StarShip.Domain.Models.Shared;

namespace StarShip.Tests;

public class SharedTests
{
    // --- 1.4 CrewMember: Factory Methods ---

    [Fact]
    public void Pilot_ShouldHaveRolePilot_WhenCreated()
    {
        // When
        var nog = CrewMember.Pilot("Nog");

        // Then
        Assert.Equal(CrewRole.Pilot, nog.Role);
        Assert.Equal("Nog", nog.Name);
    }

    // --- 2.1 Coordinate: DistanceTo ---

    [Fact]
    public void DistanceTo_ShouldReturnFive_WhenPointsAreThreeAndFourApart()
    {
        // Given
        var a = new Coordinate(0, 0);
        var b = new Coordinate(3, 4);

        // When
        double distance = a.DistanceTo(b);

        // Then
        Assert.Equal(5.0, distance);
    }
}
