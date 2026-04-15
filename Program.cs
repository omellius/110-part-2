using UniversitetsSystem;

var system = new UniversitetSystem();

// testdata så man har noe fra start
system.FyllTestData();
Console.WriteLine("Testdata fylt.\n");

bool kjør = true;

// while loopen kjører menyen til brukeren velger å avslutte
while (kjør)
{
    PrintMeny();
    string valg = (Console.ReadLine() ?? "").Trim();

    // switch case istedenfor if statements for menyvalg, kortere.
    switch (valg)
    {
        case "1":
        OpprettKurs();
        break;
        case "2":
        MeldStudentPåKurs();
        break;
        case "3":
        system.PrintAlleKurs();
        break;
        case "4":
        SøkKurs();
        break;
        case "5":
        SøkBok();
        break;
        case "6":
        LånBok();
        break;
        case "7":
        ReturnerBok();
        break;
        case "8":
        RegistrerBok();
        break;
        case "0":
        kjør = false;
        Console.WriteLine("Programmet avsluttes...");
        break;
        default:
        Feil("Ugyldig valg.");
        break;
    }
}

// ------------------
// Lokale Funksjoner 
// ------------------

void PrintMeny()
{
    Console.WriteLine("\n══════════════════════════════════");
    Console.WriteLine("       UNIVERSITETSSYSTEM");
    Console.WriteLine("══════════════════════════════════");
    Console.WriteLine("[1] Opprett kurs");
    Console.WriteLine("[2] Meld student til kurs");
    Console.WriteLine("[3] Print kurs og deltakere");
    Console.WriteLine("[4] Søk på kurs");
    Console.WriteLine("[5] Søk på bok");
    Console.WriteLine("[6] Lån bok");
    Console.WriteLine("[7] Returner bok");
    Console.WriteLine("[8] Registrer bok");
    Console.WriteLine("[0] Avslutt");
    Console.Write("\nDitt  valg: ");
}

void OpprettKurs()
{
    Console.Write("Kurskode: ");
    string kode =  Console.ReadLine() ?? ""; // ?? "" er for å sikre at kode ikke er null

    Console.Write("Kursnavn: ");
    string navn = Console.ReadLine() ?? "";

    Console.Write("Studiepoeng: ");
    // if betingelse dobbeltsjekker input typen
    if (!int.TryParse(Console.ReadLine(), out int stp))
    { 
        Feil("Ugyldig studiepoeng."); 
        return; 
    }

    Console.Write("Maks plasser: ");
    if (!int.TryParse(Console.ReadLine(), out int maks))
    {
        Feil("Ugyldig antall plasser."); 
        return;
    }

    bool ok = system.OpprettKurs(kode, navn, stp, maks);
    Svar(ok, $"Kurs {kode} {navn} er opprettet.", "Kurs med den koden finnes allerede.");
}

void MeldStudentPåKurs()
{
    Console.Write("Student-ID: ");
    string sid = Console.ReadLine() ?? "";
    Console.Write("Kurskode: ");
    string kk = Console.ReadLine() ?? "";

    (bool ok, string melding) = system.MeldStudentPåKurs(sid, kk);
    Svar(ok, melding, melding);
}

void SøkKurs()
{
    Console.Write("Søkeord (kode eller navn): ");
    string søk = Console.ReadLine() ?? "";

    // LINQ, søkkurs brukere .where() og returnerer en liste med kurs
    var resultater = system.SøkKurs(søk);

    if (!resultater.Any())
    {
        Console.WriteLine("Ingen kurs funnet.");
        return;
    }

    foreach (var kurs in resultater)
    kurs.PrintDetaljer();
}

void SøkBok()
{
    Console.Write("Søkeord (tittel, forfatter eller ID): ");
    string søk = Console.ReadLine() ?? "";

    var resultater = system.SøkBok(søk); 
    if (!resultater.Any())
    {
        Console.WriteLine("Ingen bøker funnet.");
        return;
    }

    foreach (var bok in resultater)
    Console.WriteLine($"{bok}");
}

void LånBok()
{
    Console.Write("Bruker-ID (student- eller ansatt-ID): ");
    string bid = Console.ReadLine() ?? "";
    Console.Write("Bok-ID: ");
    string bokID = Console.ReadLine() ?? "";

    (bool ok, string melding) = system.LånBok(bid, bokID);
    Svar(ok, melding, melding);
}

void ReturnerBok()
{
    Console.Write("Bruker-ID: ");
    string bid = Console.ReadLine() ?? "";
    Console.Write("Bok-ID: ");
    string bokID = Console.ReadLine() ?? "";

    (bool ok, string melding) = system.ReturnerBok(bid, bokID);
    Svar(ok, melding, melding);

    Console.WriteLine("\n Aktive lån:");
    system.PrintAktiveLån();
}

void RegistrerBok()
{
    Console.Write("Bok-ID: ");
    string id = Console.ReadLine() ?? "";
    Console.Write("Tittel: ");
    string tittel = Console.ReadLine() ?? "";
    Console.Write("Forfatter: ");
    string forfatter = Console.ReadLine() ?? "";
    Console.Write("Utgivelsesår: ");
    if (!int.TryParse(Console.ReadLine(), out int år))
    {
        Feil("Ugyldig år.");
        return;
    }
    Console.Write("Antall eksemplarer: ");
    if (!int.TryParse(Console.ReadLine(), out int antall))
    {
        Feil("Ugyldig antall.");
        return;
    }

    bool ok = system.RegistrerBok(id, tittel, forfatter, år, antall);
    Svar(ok, $"\"{tittel}\" er registrert.", "En bok med den ID-en finnes allerede.");
}

//Hjelpefunksjon for tilbakemelding
void Svar(bool ok, string okMelding, string feilMelding)
{
    if (ok) Console.WriteLine($"  ✓ {okMelding}");
    else Console.WriteLine($"  ✗ {feilMelding}");
}

void Feil(string melding) => Console.WriteLine($"  ✗ {melding}");