using StarShip.Domain.Exceptions;
using StarShip.Domain.Models;
using StarShip.Domain.Models.Shared;
using StarShip.Domain.Queries;

namespace StarShip.App;

/// <summary>
/// Die Geschichte zum Vorlesen: dotnet run -- story
/// Erzähltext steht hier, jede Zahl kommt aus der Domäne. Die Domäne schreibt nie selbst auf die Konsole.
/// </summary>
public static class Story
{
    // --- Constants ---

    private static readonly Coordinate Home = new(100, 200);
    private static readonly Coordinate XhosaTarget = new(145, 200);
    private static readonly Coordinate BajorColony = new(160, 200);
    private static readonly Coordinate BokNorRoute = new(100, 260);

    private const int TankerDelivery = 30;       // Lieferung des Tankers in Akt 1, reicht genau für drei Schiffe
    private const int ColonyGift = 10;           // Antimaterie, die die Kolonie zum Dank liefert
    private const int FuelForScout = 2;          // Bok'Nor gibt dem Aufklärer Antimaterie für den Hinweg
    private const int FuelForScoutReturn = 3;    // Rio Grande gibt ihm genug für den Rückweg, aber nicht ganz bis nach Hause
    private const int FuelForStrandedScout = 1;  // Xhosa gibt dem gestrandeten Falken einen Sprung Antimaterie

    private static readonly Coordinate ScoutStranded = new(100, 230);   // Hier geht dem Falken die Antimaterie aus

    // --- Business Methods ---

    public static void Run()
    {
        // ------ PROLOG ------
        Chapter("Prolog");
        Say("Sternzeit 49011.4. Deep Space 9 kreist am Rand des Bajoranischen Systems, direkt vor dem Wurmloch.");
        Say("Commander Sisko hält die Station mit seiner Crew am Laufen.");
        Say("Heute beginnt eine neue Versorgungsroute. Die Kolonie auf Bajor wartet auf Hilfsgüter.");
        Say("Niemand ahnt, dass der Warpkern der Xhosa seit der letzten Wartung zu heiß läuft.");

        // ------ AKT 1 ------
        Chapter("Akt 1: Die Docks öffnen");
        var ds9 = new SpaceStation("Deep Space 9", Home) { BuiltAt = new DateOnly(2351, 1, 1) };

        var kira = CrewMember.Captain("Kira");
        var worf = CrewMember.Captain("Worf");
        var nog = CrewMember.Pilot("Nog");
        var dax = CrewMember.Pilot("Dax");
        var obrien = CrewMember.Engineer("O'Brien");
        var rom = CrewMember.Engineer("Rom");
        var bashir = CrewMember.Medic("Bashir");
        var jabara = CrewMember.Medic("Jabara");
        var kasidy = CrewMember.Captain("Kasidy Yates");
        var toran = CrewMember.Captain("Toran");
        var tamar = CrewMember.Pilot("Tamar");
        var vlax = CrewMember.Captain("Vlax");
        var jorel = CrewMember.Pilot("Jorel");
        var serenade = CrewMember.Engineer("Serenade");
        foreach (var member in new[] { kira, worf, nog, dax, obrien, rom, bashir, jabara, kasidy, toran, tamar, vlax, jorel, serenade })
            ds9.AddCrew(member);
        Say($"{ds9.CrewCount} Leute melden sich mit gepackten Taschen zum Dienst: " +
            $"{ds9.CrewCountByRole(CrewRole.Captain)} Captains, {ds9.CrewCountByRole(CrewRole.Pilot)} Piloten, " +
            $"{ds9.CrewCountByRole(CrewRole.Engineer)} Ingenieure und {ds9.CrewCountByRole(CrewRole.Medic)} Ärzte.");

        ds9.Supply(TankerDelivery);
        Say($"Ein bajoranischer Tanker liefert {ds9.FuelPool} Einheiten Antimaterie.");

        var xhosa = new CargoShip("Xhosa", Home, 100) { BuiltAt = new DateOnly(2360, 5, 12) };
        var bokNor = new CargoShip("Bok'Nor", Home, 60) { BuiltAt = new DateOnly(2355, 7, 1) };
        var rioGrande = new RescueShip("Rio Grande", Home) { BuiltAt = new DateOnly(2369, 11, 20) };
        List<BaseShip> fleet = [xhosa, bokNor, rioGrande];
        foreach (var ship in fleet)
            ds9.Dock(ship);
        Say($"{ds9.ShipCount} Schiffe tauchen am Wurmloch auf und docken an: {string.Join(", ", ds9.Ships.Select(s => s.Name))}.");
        ds9.Refuel(xhosa);
        ds9.Refuel(bokNor);
        Say($"Xhosa und Bok'Nor tanken je {BaseShip.MaxFuel} Einheiten. Im Pool der Station bleiben {ds9.FuelPool}.");

        // ------ AKT 2 ------
        Chapter("Akt 2: Die Schiffe laufen aus");
        ds9.AssignCrew(xhosa);
        ds9.AssignMember(xhosa, kasidy);
        ds9.AssignMember(xhosa, rom);
        ds9.AssignMember(xhosa, serenade);
        xhosa.Load(80);
        Say("Kira und Nog gehen an Bord der Xhosa. Kasidy Yates, Rom und die junge Unteroffizierin Serenade folgen.");
        Say($"Die Kräne heben {xhosa.Cargo} Tonnen Hilfsgüter in den Laderaum. Die Xhosa ist zu {xhosa.CargoPercent} % beladen.");

        ds9.AssignMember(bokNor, toran);
        ds9.AssignMember(bokNor, tamar);
        ds9.AssignMember(bokNor, vlax);
        bokNor.Load(60);
        bokNor.LoadSpareCore();
        Say("Auf der Bok'Nor übernimmt Kapitän Toran das Kommando, mit Pilotin Tamar und Offizier Vlax.");
        Say($"Sie ist mit {bokNor.Cargo} Tonnen voll beladen. Im Laderaum liegt außerdem {bokNor.SpareCores} Ersatz-Warpkern, nur für alle Fälle.");

        xhosa.Launch();
        bokNor.Launch();
        FlyTo(bokNor, BokNorRoute);
        xhosa.WarpTo(XhosaTarget);
        xhosa.WarpTo(XhosaTarget);
        Say("Beide Frachter legen ab. Die Xhosa fliegt Richtung Bajor, die Bok'Nor eine ganz andere Route.");
        Say($"Nach zwei Sprüngen liegt die Station {xhosa.DistanceTo(ds9):0} Lichtjahre hinter der Xhosa. " +
            $"Die Bok'Nor ist schon {bokNor.DistanceTo(ds9):0} Lichtjahre weit draußen.");

        // ------ AKT 3 ------
        Chapter("Akt 3: Der Notruf");
        xhosa.WarpTo(XhosaTarget);
        Say($"Beim dritten Sprung, {xhosa.DistanceTo(ds9):0} Lichtjahre von der Station entfernt, zeigt die Temperaturanzeige der Xhosa tiefrot.");
        xhosa.CoreFailure();
        xhosa.SendDistressSignal("Warpkern überhitzt");
        Say("Ein Ruck geht durch das Schiff. Der Warpkern schaltet ab. Ein Notruf geht hinaus.");

        var nearest = fleet.Where(s => s != xhosa).Nearest(xhosa.Position);
        Say("Auf Deep Space 9 springt der Schirm auf Rot. Die Station ortet alle Schiffe.");
        Say($"Die Rio Grande liegt {rioGrande.DistanceTo(xhosa):0} Lichtjahre entfernt, {rioGrande.JumpsTo(xhosa.Position)} Warp-Sprünge. " +
            $"Die Bok'Nor ist {bokNor.DistanceTo(xhosa):0} Lichtjahre entfernt, {bokNor.JumpsTo(xhosa.Position)} Sprünge. " +
            $"Am nächsten ist die {nearest?.Name}.");

        Say("Auch die Bok'Nor empfängt den Notruf.");
        Radio(bokNor, xhosa, "Xhosa, hier Bok'Nor. Was ist passiert?");
        Radio(xhosa, bokNor, "Der Warpkern ist überhitzt und lässt sich nicht mehr abkühlen! Das ganze Schiff wird verbrennen. Wir brauchen Hilfe!");
        Say("In der Crew der Xhosa breitet sich Panik aus.");
        Say($"Die Bok'Nor bräuchte {bokNor.JumpsTo(xhosa.Position)} Sprünge. Sie käme nie rechtzeitig an. Also hilft Toran mit Ideen.");
        Radio(bokNor, xhosa, "Versammle deine Crew. Schick ein paar Leute in die Küche, sie sollen Wasser einfrieren.");
        Radio(bokNor, xhosa, "Lass prüfen, ob alle da sind. Dann kontrolliert die Rettungsboote.");

        Say(xhosa.MissingRoles().Count == 0
            ? $"Die Xhosa zählt ihre Leute: Alle {xhosa.CrewCount} sind da, niemand fehlt."
            : $"Die Xhosa zählt ihre Leute: {xhosa.CrewCount} sind da, es fehlen: {string.Join(", ", xhosa.MissingRoles())}.");
        Say($"Dann die Rettungsboote: {BaseShip.Lifeboats} Boote mit je {BaseShip.SeatsPerLifeboat} Plätzen sind einsatzbereit. " +
            $"Das sind {xhosa.LifeboatSeats} Plätze für {xhosa.CrewCount} Leute. {xhosa.CrewWithoutLifeboatSeat} hätte keinen Platz.");
        Radio(bokNor, xhosa, "Bringt das Eis zum Warpkern und legt es vor die Tür. Das bremst Hitze und Feuer.");
        Say("Die Crew schleppt Eisblock um Eisblock zum Maschinenraum.");

        // ------ AKT 4 ------
        Chapter("Akt 4: Die Rio Grande fliegt");
        ds9.AssignCrew(rioGrande);
        ds9.AssignMember(rioGrande, jorel);
        ds9.Refuel(rioGrande);
        Say("Auch die Rio Grande empfängt den Notruf. Eine Sirene ruft die Crew.");
        Say($"Nacheinander treffen {rioGrande.CrewCount} Leute ein: {string.Join(", ", rioGrande.Crew.Select(c => c.Name))}.");
        Say($"Die Station füllt den Warpkern der Rio Grande mit {BaseShip.MaxFuel} Einheiten. Im Pool bleiben {ds9.FuelPool}.");

        var report = fleet.DistressReports().Single();
        Say($"Der Notruf wird analysiert: Die {report.Name} meldet „{report.Reason}“, " +
            $"{rioGrande.DistanceTo(xhosa):0} Lichtjahre entfernt, gesendet um {report.SentAt:HH:mm} Uhr.");
        Say(rioGrande.HasRequiredRoles
            ? "Der Eingangsscanner meldet: Captain, Pilot und Arzt sind an Bord. Die Crew ist vollständig."
            : $"Der Eingangsscanner meldet: Es fehlen {string.Join(", ", rioGrande.MissingRoles())}.");
        rioGrande.Launch();
        Say("Die Rio Grande legt ab.");

        Say("Kurz darauf funkt die Bok'Nor.");
        Radio(bokNor, rioGrande, "Wir haben einen Ersatz-Warpkern an Bord. Braucht ihr den?");
        Radio(rioGrande, bokNor, "Und wie! Wir berechnen eine Übergabe.");
        int direct = rioGrande.JumpsTo(xhosa.Position);
        int detour = rioGrande.JumpsTo(bokNor.Position) + rioGrande.JumpsBetween(bokNor.Position, xhosa.Position);
        Say($"Der Bordcomputer rechnet: Direkt wären es {direct} Sprünge zur Xhosa. Mit dem Umweg über die Bok'Nor sind es {detour}.");

        Say("Da hat Toran noch eine Idee.");
        Radio(bokNor, rioGrande, "Ich schicke euch ein Aufklärungsschiff. Es ist kleiner, leichter und schneller.");
        var falke = new ScoutShip("Falke", bokNor.Position) { BuiltAt = new DateOnly(2371, 3, 8) };
        fleet.Add(falke);
        bokNor.Transfer(vlax, falke);
        bokNor.TransferFuel(falke, FuelForScout);
        bokNor.HandOverSpareCore(falke);
        Say($"Offizier Vlax steigt in den Falken. Die Bok'Nor gibt ihm {falke.Fuel} Einheiten Antimaterie und den Warpkern mit.");
        int scoutJumps = falke.JumpsTo(rioGrande.Position);
        Say($"Der Falke springt {falke.JumpRange} Lichtjahre weit, doppelt so weit wie die Rio Grande. " +
            $"Vlax braucht nur {scoutJumps} Sprünge statt {detour}.");
        FlyTo(falke, rioGrande.Position);
        falke.HandOverSpareCore(rioGrande);
        Say("Vlax dockt an der Rio Grande an und übergibt den Kern.");
        rioGrande.TransferFuel(falke, FuelForScoutReturn);
        Say($"Die Rio Grande tankt den Falken mit {FuelForScoutReturn} Einheiten auf. Vlax fliegt zurück zur Bok'Nor.");
        FlyTo(falke, bokNor.Position);

        Radio(rioGrande, xhosa, "Xhosa, wir kommen. Und wir bringen einen Ersatz-Warpkern!");
        Say($"Die Rio Grande nimmt direkten Kurs auf die Xhosa: {rioGrande.JumpsTo(xhosa.Position)} Sprünge.");
        FlyTo(rioGrande, xhosa.Position);

        // ------ AKT 5 ------
        Chapter("Akt 5: Die Reparatur");
        Say("Kira versucht verzweifelt, die Crew ruhig zu halten. Dann sieht sie die Rio Grande auf dem Monitor. Das beruhigt sie sehr.");
        Say($"Ein Rettungsschlauch verbindet die beiden Schiffe. Die {BaseShip.Lifeboats} Rettungsboote werden nicht gebraucht.");

        rioGrande.TakeInjured(xhosa, nog);
        rioGrande.TakeInjured(xhosa, rom);
        Say("Nog und Rom sind verletzt. Bashir holt die beiden auf die Krankenstation der Rio Grande.");
        rioGrande.Transfer(dax, xhosa);
        Say($"Dax wechselt als Pilotin auf die Xhosa. Jetzt sind {xhosa.CrewCount} Leute auf der Xhosa " +
            $"und {rioGrande.CrewCount} von {BaseShip.MaxCrew} Plätzen auf der Rio Grande besetzt.");

        Say("Das Eis hat das Schlimmste verhindert. Die Crew räumt die Eisreste weg und macht den Weg zum Warpkern frei.");
        rioGrande.HandOverSpareCore(xhosa);
        xhosa.ReplaceCore();
        Say("Beide Crews bauen gemeinsam den Ersatzkern ein.");

        xhosa.FailControlComputer();
        Say("Doch dann fällt der Kontrollcomputer aus.");
        try
        {
            xhosa.ReleaseCore();
        }
        catch (StarshipException ex)
        {
            Say($"Der Bordcomputer meldet: „{ex.Message}“");
        }
        Say("Ohne Sicherheitsfreigabe startet kein Warpkern. Kira wird unruhig.");
        Say("Da hat Serenade eine Idee. Sie bastelt gern. Schnell lötet sie ein paar Teile zusammen und verbindet sie mit dem Prüfgerät.");
        xhosa.ReleaseCoreManually(serenade);
        xhosa.ClearDistressSignal();
        Say("Es klappt! Der Warpkern ist freigegeben, der Notruf wird aufgehoben.");

        // ------ AKT 6 ------
        Chapter("Akt 6: Auftrag erfüllt, doch der Heimweg ist lang");
        var bajor = new SpaceStation("Kolonie Bajor", BajorColony);
        int tons = xhosa.Cargo;
        Say($"Die Xhosa beendet ihren Auftrag. Die Kolonie auf Bajor liegt nur noch {xhosa.JumpsTo(bajor.Position)} Sprung entfernt.");
        xhosa.WarpTo(bajor);
        xhosa.Unload(tons);
        Say($"Sie dockt an und lädt {tons} Tonnen Hilfsgüter ab.");
        bajor.Supply(ColonyGift);
        bajor.Refuel(xhosa);
        Say($"Zum Dank füllt die Kolonie den Warpkern der Xhosa wieder auf {xhosa.Fuel} Einheiten.");
        xhosa.Launch();

        Say("Dann fliegen alle Schiffe nach Hause.");
        foreach (var ship in new BaseShip[] { bokNor, rioGrande })
            FlyHomeAndReport(ship, ds9);

        Say($"Auch Vlax startet mit dem Falken. Er hat nur noch {falke.Fuel} Einheit Antimaterie, " +
            $"für den Heimweg braucht er {falke.JumpsTo(ds9.Position)}.");
        Say(falke.CanWarpTo(ds9)
            ? "Der Bordcomputer meldet: Die Antimaterie reicht."
            : "Der Bordcomputer meldet: Die Antimaterie reicht nicht bis Deep Space 9. Vlax fliegt trotzdem so weit er kann.");
        int strandJumps = falke.JumpsTo(ScoutStranded);
        FlyTo(falke, ScoutStranded);
        Say($"Nach {strandJumps} Sprung ist der Tank leer. " +
            $"Der Falke treibt {falke.DistanceTo(ds9):0} Lichtjahre vor der Station im All, nur {falke.JumpsTo(ds9.Position)} Sprung von zu Hause.");
        Radio(falke, ds9, "Deep Space 9, hier Falke. Mein Tank ist leer. Ich komme nicht mehr nach Hause.");
        Radio(ds9, bokNor, "Bok'Nor, könnt ihr dem Falken helfen?");
        Say($"Doch der Pool der Station ist leer: {ds9.FuelPool} Einheiten. Die Bok'Nor hat {bokNor.Fuel} Einheiten übrig.");
        Say($"Die Rio Grande hat noch {rioGrande.Fuel} Einheit. Bis zum Falken bräuchte sie {rioGrande.JumpsTo(falke.Position)} Sprünge, " +
            "denn sie springt nur halb so weit wie er.");

        Say($"Da meldet sich die Xhosa. Sie hat noch {xhosa.Fuel} Einheiten im Kern.");
        Radio(xhosa, falke, "Falke, hier Xhosa. Ihr habt uns gerettet. Jetzt holen wir euch!");
        int xhosaJumps = xhosa.JumpsTo(falke.Position);
        FlyTo(xhosa, falke.Position);
        xhosa.TransferFuel(falke, FuelForStrandedScout);
        Say($"Nach {xhosaJumps} Sprüngen erreicht die Xhosa den Falken. Sie gibt ihm {FuelForStrandedScout} Einheit Antimaterie. " +
            "So hilft das gerettete Schiff jetzt seinem Retter.");

        Say("Seite an Seite fliegen die beiden nach Hause.");
        foreach (var ship in new BaseShip[] { falke, xhosa })
            FlyHomeAndReport(ship, ds9);
        Say(fleet.AllDocked()
            ? $"Alle {fleet.Count} Schiffe sind zurück an Deep Space 9."
            : "Noch sind nicht alle Schiffe zurück.");

        // ------ AKT 7 ------
        Chapter("Akt 7: Die Ursache");
        Say("Nog und Rom kommen auf die Krankenstation der Station. Bashir meldet: beide auf dem Weg der Besserung.");
        Say("Doch Rom kann nicht stillliegen. Er lässt sich den alten Warpkern der Xhosa ans Bett bringen.");
        var failure = xhosa.Logbook.ByType(LogType.Repair).First(e => e.Message.Contains("überhitzt"));
        Say($"Im Logbuch der Xhosa steht um {failure.At:HH:mm} Uhr: „{failure.Message}“.");
        Say("Rom schraubt die Kühlung auf. Ein Kühlventil ist falsch herum eingebaut. Das ist bei der letzten Wartung passiert.");
        Say("Der Kern konnte seine Hitze nie richtig abgeben. Er lief vom ersten Sprung an zu heiß.");
        Say($"O'Brien ordnet sofort eine Prüfung an. Alle {ds9.ShipCount} angedockten Schiffe werden kontrolliert: " +
            $"{string.Join(", ", ds9.Ships.Select(s => s.Name))}.");
        Say("Bei keinem anderen Schiff findet sich der Fehler. Ab jetzt prüfen zwei Techniker jede Wartung.");

        // ------ EPILOG ------
        Chapter("Epilog: Das Fest");
        int radioCount = fleet.Sum(s => s.Logbook.ByType(LogType.Radio).Count(e => e.Message.StartsWith("Funk an")))
                         + ds9.Logbook.ByType(LogType.Radio).Count(e => e.Message.StartsWith("Funk an"));
        Say($"Sisko gibt ein großes Fest. {radioCount} Funksprüche, ein Ersatzkern und eine gelötete Schaltung haben die Xhosa gerettet.");
        Say("Dann ehrt Sisko drei Menschen.");

        int toranRadios = bokNor.Logbook.ByType(LogType.Radio).Count(e => e.Message.StartsWith("Funk an"));
        Say($"Kapitän {toran.Name} von der Bok'Nor: Er hat mit {toranRadios} Funksprüchen die Rettung geleitet, ohne selbst hinzufliegen.");
        var release = xhosa.Logbook.ByType(LogType.Repair).Last(e => e.Message.Contains("Behelfsprüfung"));
        Say($"Unteroffizierin {serenade.Name} von der Xhosa: Im Logbuch steht „{release.Message}“. Ihre Lötarbeit hat den Kern gestartet.");
        int scoutJumpsTotal = falke.Logbook.ByType(LogType.Warp).Count;
        Say($"Captain {vlax.Name} vom Falken: {scoutJumpsTotal} Sprünge mit dem kleinsten Schiff der Flotte, den Ersatzkern sicher übergeben.");
        Say("Das Fest belohnt die Zusammenarbeit und stärkt die Verbindung zwischen den Schiffen.");

        Say("Die Musik spielt. Da piept es auf der Kommandozentrale.");
        Say("Ein Funksignal kommt herein. Man hört nur Rauschen.");
        Console.WriteLine("   📡 ??? an Deep Space 9: „…krrrz… hier… Tal'Mira… krrrz… bitte… krrrz…“");
        Say($"Dahinter eine leise Stimme. Niemand versteht die Worte. Das Schiff ist weiter weg als die {BaseShip.SensorRange} Lichtjahre der Sensoren.");
        Say("Es wird still im Raum. Sisko stellt sein Glas ab und schaut in die Runde.");
        Say("Kira, Toran und Vlax stehen schon auf.");
        Say("Fortsetzung folgt …");

        Chapter("Logbuch der Xhosa");
        Console.WriteLine(xhosa.Logbook.Report());
    }

    // --- Helpers ---

    private static void Chapter(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static void Say(string text) => Console.WriteLine(text);

    private static void Radio(BaseEntity from, BaseEntity to, string message)
    {
        from.Radio(to, message);
        Console.WriteLine($"   📡 {from.Name} an {to.Name}: „{message}“");
    }

    private static void FlyTo(BaseShip ship, Coordinate target)
    {
        while (ship.Position != target)
            ship.WarpTo(target);
    }

    private static void FlyHome(BaseShip ship, SpaceStation station)
    {
        while (!ship.IsDocked)
            ship.WarpTo(station);
    }

    private static void FlyHomeAndReport(BaseShip ship, SpaceStation station)
    {
        int jumps = ship.JumpsTo(station.Position);
        FlyHome(ship, station);
        string article = ship is ScoutShip ? "Der" : "Die";
        string jumpWord = jumps == 1 ? "Sprung" : "Sprünge";
        string fuelText = ship.Fuel switch
        {
            0 => "mit dem letzten Tropfen Antimaterie",
            1 => "mit 1 Einheit Antimaterie",
            _ => $"mit {ship.Fuel} Einheiten Antimaterie"
        };
        Say($"{article} {ship.Name} braucht {jumps} {jumpWord} und dockt {fuelText} an.");
    }
}