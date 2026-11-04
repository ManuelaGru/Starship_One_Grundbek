using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;
using StarShip.Domain.Queries;

// ------ STORY ------
// dotnet run -- story erzählt die eigene Geschichte, ohne Argument kommt die Ausgabe aus dem Guide
if (args.Contains("story"))
{
    StarShip.App.Story.Run();
    return;
}

// ------ AKT 1 ------
Console.WriteLine("=== AKT 1: Die Docks öffnen ===");

// Sternzeit 49011.4, die neue Deep Space 9 geht in Betrieb. Die Station steht bei (100,200), gebaut 2351.
var home = new Coordinate(100, 200);
var ds9 = new SpaceStation("Deep Space 9", home) { BuiltAt = new DateOnly(2351, 1, 1) };

// Acht Leute melden sich zum Dienst und warten im Pool. Die Reihenfolge zählt.
ds9.AddCrew(CrewMember.Captain("Kira"));
ds9.AddCrew(CrewMember.Captain("Worf"));
ds9.AddCrew(CrewMember.Pilot("Nog"));
ds9.AddCrew(CrewMember.Pilot("Dax"));
ds9.AddCrew(CrewMember.Engineer("O'Brien"));
ds9.AddCrew(CrewMember.Engineer("Rom"));
ds9.AddCrew(CrewMember.Medic("Bashir"));
ds9.AddCrew(CrewMember.Medic("Jabara"));

// Der Tanker liefert 40 Einheiten Antimaterie.
ds9.Supply(40);

// Drei Schiffe tauchen am Wurmloch auf und stehen bei home. fleet hält alle drei.
var xhosa = new CargoShip("Xhosa", home, 100) { BuiltAt = new DateOnly(2360, 5, 12) };
var bokNor = new CargoShip("Bok'Nor", home, 60) { BuiltAt = new DateOnly(2355, 7, 1) };
var rioGrande = new RescueShip("Rio Grande", home) { BuiltAt = new DateOnly(2369, 11, 20) };
List<BaseShip> fleet = [xhosa, bokNor, rioGrande];

// Alle drei docken an. Xhosa und Bok'Nor tanken voll, die Rio Grande noch nicht.
ds9.Dock(xhosa);
ds9.Dock(bokNor);
ds9.Dock(rioGrande);
ds9.Refuel(xhosa);
ds9.Refuel(bokNor);

// Sisko will Zahlen sehen.
Console.WriteLine($"Piloten im Pool: {ds9.CrewCountByRole(CrewRole.Pilot)}");
Console.WriteLine($"Crew je Rolle: {string.Join(", ", ds9.CrewCountPerRole())}");

// Zwei Kacheln auf dem Hauptschirm.
Console.WriteLine();
Console.WriteLine(ds9.Report());
Console.WriteLine();
Console.WriteLine(xhosa.Report());

// ------ AKT 2 ------
Console.WriteLine();
Console.WriteLine("=== AKT 2: Xhosa läuft aus ===");

// Die Station besetzt die Xhosa, dann heben die Kräne die Fracht.
ds9.AssignCrew(xhosa);
xhosa.Load(80);
bokNor.Load(60);

// Wer ist startklar?
Console.WriteLine($"Startklar: {string.Join(", ", fleet.ReadyToLaunch())}");

// Die Xhosa legt ab, zwei Sprünge heute.
xhosa.Launch();
var target = new Coordinate(145, 200);
xhosa.WarpTo(target);
xhosa.WarpTo(target);

// Letzter Eintrag im Logbuch der Xhosa.
Console.WriteLine($"Letzter Eintrag: {xhosa.Logbook.Last?.Message}");

// Sisko fragt die Flotte.
Console.WriteLine($"Im All: {string.Join(", ", fleet.InSpace().Select(s => s.Name))}");
Console.WriteLine($"Fracht gesamt: {fleet.TotalCargo()} t");
Console.WriteLine($"Antimaterie je Typ: {string.Join(", ", fleet.TotalFuelByType())}");

// Die Kachel der Xhosa, jetzt mit Cargo.
Console.WriteLine();
Console.WriteLine(xhosa.Report());

// ------ AKT 3 ------
Console.WriteLine();
Console.WriteLine("=== AKT 3: Notruf ===");

// Der dritte Sprung, dann der Notruf.
xhosa.WarpTo(target);
xhosa.SendDistressSignal("Warpkern überhitzt");

// Vier Fragen an die Flotte.
Console.WriteLine($"Schiff in Not: {fleet.AnyInDistress()}");
Console.WriteLine($"In Not: {string.Join(", ", fleet.InDistress().Select(s => s.Name))}");
Console.WriteLine($"Nächstes Schiff zur Station: {fleet.Nearest(home)?.Name}");
Console.WriteLine($"Notrufe: {string.Join(", ", fleet.DistressReports())}");

// Lagebericht mit Describe.
Console.WriteLine("Lagebericht:");
foreach (var ship in fleet)
    Console.WriteLine($"  {Describe(ship)}");

// ------ AKT 4 ------
Console.WriteLine();
Console.WriteLine("=== AKT 4: Rio Grande fliegt ===");

// Alarm in Quarks Bar: Crew, Antimaterie, Start, zwei Sprünge Richtung Xhosa.
ds9.AssignCrew(rioGrande);
ds9.Refuel(rioGrande);
rioGrande.Launch();
rioGrande.WarpTo(xhosa.Position);
rioGrande.WarpTo(xhosa.Position);

// Der Scan: nur Schiffe in Not.
foreach (var hit in rioGrande.Scan(fleet))
    Console.WriteLine($"Scan: {hit}");

// Wer ist überhaupt in Sensorreichweite?
var inRange = fleet
    .Where(s => s != rioGrande)
    .WithinRange(rioGrande.Position, BaseShip.SensorRange);
Console.WriteLine($"In Reichweite der Rio Grande: {string.Join(", ", inRange.Select(s => s.Name))}");

// Der dritte Sprung, dann die Rettung.
rioGrande.WarpTo(xhosa.Position);
rioGrande.Rescue(xhosa, ds9);

// Bashir liest die Logbücher.
Console.WriteLine($"Notruf-Einträge der Xhosa: {xhosa.Logbook.ByType(LogType.Distress).Count}");
Console.WriteLine($"Letzte Rettung: {rioGrande.Logbook.LastOf(LogType.Rescue)?.Message}");
Console.WriteLine($"Logbuch Rio Grande je Typ: {string.Join(", ", rioGrande.Logbook.CountByType())}");

// Zwei Kacheln.
Console.WriteLine();
Console.WriteLine(rioGrande.Report());
Console.WriteLine();
Console.WriteLine(xhosa.Report());

// ------ AKT 5 ------
Console.WriteLine();
Console.WriteLine("=== AKT 5: Heimkehr und Bilanz ===");

// Drei Sprünge nach Hause, beim dritten dockt sie von selbst an.
for (int jump = 1; jump <= 3; jump++)
{
    rioGrande.WarpTo(ds9);
    Console.WriteLine($"Warp-Sprung {jump}: {rioGrande}");
}

// Sisko fragt die Flotte.
Console.WriteLine($"Ältestes Schiff: {fleet.Oldest()?.Name}");
Console.WriteLine($"Größte Crew: {fleet.WithLargestCrew()?.Name}");
Console.WriteLine($"Schiffe je Baujahr: {string.Join(", ", fleet.PerBuildYear())}");

// Crew zurück in den Pool, Rio Grande für morgen volltanken.
ds9.UnassignCrew(rioGrande);
ds9.Refuel(rioGrande);

// Sisko fragt die Station.
Console.WriteLine($"Alle Schiffe bereit: {ds9.AllShipsReady()}");
Console.WriteLine($"Schiffe je Typ: {string.Join(", ", ds9.ShipCountByType())}");
Console.WriteLine($"Crew je Rolle: {string.Join(", ", ds9.CrewCountPerRole())}");

// Zwei Einträge aus dem Logbuch der Rio Grande.
Console.WriteLine($"Erster Eintrag: {rioGrande.Logbook.First?.Message}");
Console.WriteLine($"Rettung: {rioGrande.Logbook.ByType(LogType.Rescue).SingleOrDefault()?.Message}");

// Das ganze Logbuch und der Bericht der Station.
Console.WriteLine();
Console.WriteLine(rioGrande.Logbook.Report());
Console.WriteLine();
Console.WriteLine(ds9.Report());

// ------ HELPERS ------

// Akt 3: ein Satz je Schiff, passend zum Typ. Die erste passende Zeile gewinnt.
static string Describe(BaseShip ship) => ship switch
{
    CargoShip { Status: ShipStatus.Distress } c  => $"{c.Name}: Frachter {c.CargoPercent} % beladen, NOTRUF",
    CargoShip { IsFull: true } c                  => $"{c.Name}: Frachter voll beladen, {c.Status}, {c.Readiness}",
    CargoShip c                                   => $"{c.Name}: Frachter {c.CargoPercent} % beladen, {c.Status}, {c.Readiness}",
    RescueShip { Readiness: Readiness.Ready } r   => $"{r.Name}: Rettungsschiff startklar, {r.Status}",
    RescueShip r                                  => $"{r.Name}: Rettungsschiff, {r.Status}, {r.Readiness}",
    _                                             => ship.ToString()
};
