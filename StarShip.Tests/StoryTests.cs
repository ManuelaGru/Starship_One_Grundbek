using StarShip.App;
using Xunit.Abstractions;

namespace StarShip.Tests;

/// <summary>Szenario-Test: spielt die ganze eigene Geschichte durch und zeigt sie in der Testausgabe.</summary>
public class StoryTests(ITestOutputHelper output)
{
    [Fact]
    public void Run_ShouldBringAllShipsHome_WhenWholeStoryIsTold()
    {
        // Given
        var console = new StringWriter();
        var original = Console.Out;
        Console.SetOut(console);

        // When
        try
        {
            Story.Run();
        }
        finally
        {
            Console.SetOut(original);
        }

        // Then
        string story = console.ToString();
        output.WriteLine(story);
        Assert.Contains("Alle 4 Schiffe sind zurück an Deep Space 9.", story);
        Assert.Contains("lädt 80 Tonnen Hilfsgüter ab", story);
        Assert.Contains("Sicherheitsfreigabe per Behelfsprüfung von Serenade (Engineer)", story);
    }
}
