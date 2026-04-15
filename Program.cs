using UniversitetsSystem;

var system = new UniversitetSystem();

// testdata så man har noe fra start
system.FyllTestData();
Console.WriteLine("Testdata fylt.\n");

bool kjør = true;


// Applikasjonen starter nå alltid i autentiseringsflyt
// Bruker må logge inn eller registrere seg før rollemeny vises
while (kjør)
{
    try
    {
        Bruker? aktivBruker = VisInnloggingsMeny();
        if (aktivBruker == null)
        {
            kjør = false; 
            continue;
        }
        KjørRolleMeny(aktivBruker);
    }
    catch (Exception ex)
    {
        Feil($"Uventet feil: {ex.Message}");
    }
}

Console.WriteLine("Programmet avsluttes...");

Bruker? VisInnloggingsMeny()
{
    // Felles inngangsport som skiller mellom eksisterende og nye brukere
    Console.WriteLine("\n══════════════════════════════════");
    Console.WriteLine("   LOGG INN / REGISTRER BRUKER");
    Console.WriteLine("══════════════════════════════════");
    Console.WriteLine("[1] Eksisterende bruker");
    Console.WriteLine("[2] Ny bruker");
    Console.WriteLine("[0] Avslutt");

    string? valg = PromptValg("Ditt valg: ", "Ugyldig valg.", "1", "2", "0");
    if (valg == "0") return null;
    if (valg == 2)
    {
        RegistrerBruker();
        return null;
    }
    return LoggInnBruker();
}

Bruker? LoggInnBruker()
{
    // Innlogging er flyttet til en egen funksjon for mer tydelig ansvar
    // Program.cs samler inputten, universitetssystem verifiserer input
    string brukernavn = PromptIkkeTom("Brukernavn: ", false)!;
    string passord = PromptIkkeTom("Passord: ", false)!;
    var (ok, melding, bruker) = system.LoggInn(brukernavn, passord);
    Svar(ok, melding, melding);
    return bruker;
}

void RegistrerBruker()
{
    // Registrering støtter rollevalg, slik at riktig meny vises etter innlogging.
    Console.WriteLine("\nVelg rolle: ");
    Console.WriteLine("[1] Student");
    Console.WriteLine("[2] Faglærer");
    Console.WriteLine("[3] BibliotekAnsatt");
    string? rolleValg = PromptValg("Ditt valg: ", "Ugyldig valg.", "1", "2", "3");
    BrukerRolle rolle = rolleValg switch
    {
        "1" => BrukerRolle.Student,
        "2" => BrukerRolle.Faglærer,
        _ => BrukerRolle.BibliotekAnsatt
    };

    string prefix = rolle == BrukerRolle.Student ? "S" : "A";
    string id = PromptIkkeTom($"ID (eks. {prefix}003): ", true)!;
    string navn = PromptIkkeTom("Navn: ", false)!;
    string epost = PromptIkkeTom("Brukernavn: ", false)!;
    string passord = PromptIkkeTom("Passord: ", false)!;

    var(ok, melding) = system.RegistrerNyBruker(rolle, id, navn, epost, brukernavn, passord);
    Svar(ok, melding, melding);)
}

void KjørRolleMeny(Bruker bruker)
{
    // Rollebasert tilgangskontroll i UI
    //Hver rolle får kun funksjonene oppgaven krever
    bool loggetInn = true;
    while (loggetInn)
    {
        if (bruker.Rolle == BrukerRolle.Student && bruker is Student student)
            loggetInn = StudentMeny(student);
        else if (bruker.Rolle == BrukerRolle.BibliotekAnsatt && bruker is Ansatt bibliotekar)
            loggetInn = BibliotekMeny(bibliotekar);
        else if (bruker.Rolle == BrukerRolle.Faglærer && bruker is Ansatt faglærer)
            loggetInn = FaglærerMeny(faglærer);
        else
            loggetInn = false;
    }


//studentfunksjoner samlet i egen meny

bool StudentMeny(Student student)
{
    Console.WriteLine($"\n--- STUDENTMENY ({student.Navn}) ---");
    Console.WriteLine("[1] Meld på/av kurs");
    Console.WriteLine("[2] Se mine kurs");
    Console.WriteLine("[3] Se karakterer");
    Console.WriteLine("[4] Søk bok");
    Console.WriteLine("[5] Lån bok");
    Console.WriteLine("[6] Returner bok");
    Console.WriteLine("[0] Logg ut");
    string? valg = PromptValg("Ditt valg: ", "Ugyldig valg.", "1", "2", "3", "4", "5", "6", "0");

    switch (valg)
    {
        case "1": MeldStudentPåEllerAvKurs(student.StudentID); break;
        case "2": PrintKurs(system.HentKursForStudent(student.StudentID)); break;
        case "3": PrintKarakterer(student.StudentID); break;
        case "4": SøkBok(); break;
        case "5": LånBok(student.StudentID); break;
        case "6": ReturnerBok(student.StudentID); break;
        case "0": return false;
    }
    return true;
}

//Faglærer har utvidet ansvar med å sette karakterer og registrere pensum
bool FaglaererMeny(Ansatt lærer)
{
    Console.WriteLine($"\n--- FAGLÆRERMENY ({lærer.Navn}) ---");
    Console.WriteLine("[1] Opprett kurs");
    Console.WriteLine("[2] Søk kurs");
    Console.WriteLine("[3] Søk bok");
    Console.WriteLine("[4] Lån bok");
    Console.WriteLine("[5] Returner bok");
    Console.WriteLine("[6] Sett karakter");
    Console.WriteLine("[7] Registrer pensum");
    Console.WriteLine("[8] Mine kurs");
    Console.WriteLine("[0] Logg ut");
    string? valg = PromptValg("Ditt valg: ", "Ugyldig valg.", "1", "2", "3", "4", "5", "6", "7", "8", "0");

    switch (valg)
    {
        case "1": OpprettKurs(lærer.AnsattID); break;
        case "2": SøkKurs(); break;
        case "3": SøkBok(); break;
        case "4": LånBok(lærer.AnsattID); break;
        case "5": ReturnerBok(lærer.AnsattID); break;
        case "6": SettKarakter(lærer.AnsattID); break;
        case "7": RegistrerPensum(lærer.AnsattID); break;
        case "8": PrintKurs(system.HentKursForFaglaerer(lærer.AnsattID)); break;
        case "0": return false;
    }
    return true;
}

//Bibliotekansatt får kun bibliotek relaterte funksjoenr
bool BibliotekMeny(Ansatt bibliotekar)
{
    Console.WriteLine($"\n--- BIBLIOTEKMENY ({bibliotekar.Navn}) ---");
    Console.WriteLine("[1] Registrer bok");
    Console.WriteLine("[2] Se aktive lån");
    Console.WriteLine("[3] Se lånehistorikk");
    Console.WriteLine("[0] Logg ut");
    string? valg = PromptValg("Ditt valg: ", "Ugyldig valg.", "1", "2", "3", "0");
    switch (valg)
    {
        case "1": RegistrerBok(); break;
        case "2": system.PrintAktiveLån(); break;
        case "3": system.PrintLånHistorikk(); break;
        case "0": return false;
    }
    return true;
}

// Opprett kurs er nå knyttet til faglærer ID
void OpprettKurs(string faglærerId)
{
    string kode = PromptIkkeTom("Kurskode: ", true)!;
    string navn = PromptIkkeTom("Kursnavn: ", false)!;
    int stp = PromptHeltall("Studiepoeng: ", 1)!.Value;
    int maks = PromptHeltall("Maks plasser: ", 1)!.Value;
    bool ok = system.OpprettKurs(kode, navn, stp, maks, faglaererId);
    Svar(ok, $"Kurs {kode} er opprettet.", "Kurskode/navn finnes allerede eller du har feil rolle.");
}

void MeldStudentPåEllerAvKurs(string studentID)
{
    Console.WriteLine("[1] Meld på kurs");
    Console.WriteLine("[2] Meld av kurs");
    string? valg = PromptValg("Ditt valg: ", "Ugyldig valg.", "1", "2");
    string kode = PromptIkkeTom("Kurskode: ", true)!;
    var result = valg == "1" ? system.MeldStudentPåKurs(studentID, kode) : system.MeldStudentAvKurs(studentID, kode);
    Svar(result.suksess, result.melding, result.melding);
}

void SøkKurs() => PrintKurs(system.SøkKurs(PromptIkkeTom("Søkeord: ", false)!));

void SøkBok()
{
    var bøker = system.SøkBok(PromptIkkeTom("Søkeord bok: ", false)!);
    if (!bøker.Any()) Console.WriteLine("Ingen bøker funnet.");
    foreach (var bok in bøker) Console.WriteLine($"  {bok}");
}

void LånBok(string brukerId)
{
    string bokId = PromptIkkeTom("Bok-ID: ", true)!;
    var (ok, melding) = system.LånBok(brukerId, bokId);
    Svar(ok, melding, melding);
}

void ReturnerBok(string brukerId)
{
    string bokId = PromptIkkeTom("Bok-ID: ", true)!;
    var (ok, melding) = system.ReturnerBok(brukerId, bokId);
    Svar(ok, melding, melding);
}

void RegistrerBok()
{
    string id = PromptIkkeTom("Bok-ID: ", true)!;
    string tittel = PromptIkkeTom("Tittel: ", false)!;
    string forfatter = PromptIkkeTom("Forfatter: ", false)!;
    int år = PromptHeltall("Utgivelsesår: ", 1, DateTime.Now.Year)!.Value;
    int antall = PromptHeltall("Antall eksemplarer: ", 1)!.Value;
    bool ok = system.RegistrerBok(id, tittel, forfatter, år, antall);
    Svar(ok, "Bok registrert.", "Bok-ID finnes allerede.");
}

void SettKarakter(string faglaererId)
{
    // NEW: Karaktersetting går via domenelaget som også verifiserer lærerens eierskap til kurs.
    string kurskode = PromptIkkeTom("Kurskode: ", true)!;
    string studentId = PromptIkkeTom("Student-ID: ", true)!;
    string karakter = PromptIkkeTom("Karakter (A-F): ", false)!;
    var (ok, melding) = system.SettKarakter(faglaererId, kurskode, studentId, karakter);
    Svar(ok, melding, melding);
}

void RegistrerPensum(string faglaererId)
{
    // NEW: Pensum registreres kun av ansvarlig faglærer for kurset.
    string kurskode = PromptIkkeTom("Kurskode: ", true)!;
    string pensum = PromptIkkeTom("Pensumtekst: ", false)!;
    var (ok, melding) = system.RegistrerPensum(faglaererId, kurskode, pensum);
    Svar(ok, melding, melding);
}

void PrintKarakterer(string studentId)
{
    var karakterer = system.HentStudentKarakterer(studentId);
    if (!karakterer.Any())
    {
        Console.WriteLine("Ingen kurs/karakterer funnet.");
        return;
    }
    foreach (var k in karakterer)
        Console.WriteLine($"{k.kurskode} - {k.kursnavn}: {k.karakter ?? "Ikke satt"}");
}

void PrintKurs(List<Kurs> kursListe)
{
    if (!kursListe.Any())
    {
        Console.WriteLine("Ingen kurs funnet.");
        return;
    }
    foreach (var kurs in kursListe) kurs.PrintDetaljer();
}

void Svar(bool ok, string okMelding, string feilMelding) =>
    Console.WriteLine(ok ? $"✓ {okMelding}" : $"✗ {feilMelding}");
void Feil(string melding) => Console.WriteLine($"✗ {melding}");

string? PromptValg(string prompt, string feilMelding, params string[] gyldigeVerdier)
{
    while (true)
    {
        Console.Write(prompt);
        string verdi = (Console.ReadLine() ?? "").Trim();
        if (gyldigeVerdier.Contains(verdi)) return verdi;
        Feil(feilMelding);
    }
}

string? PromptIkkeTom(string prompt, bool normaliserSomId)
{
    while (true)
    {
        Console.Write(prompt);
        string verdi = (Console.ReadLine() ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(verdi))
            return normaliserSomId ? verdi.ToUpperInvariant() : verdi;
        Feil("Feltet kan ikke være tomt.");
    }
}

int? PromptHeltall(string prompt, int min, int? max = null)
{
    while (true)
    {
        Console.Write(prompt);
        if (!int.TryParse(Console.ReadLine(), out int verdi))
        {
            Feil("Må være et heltall.");
            continue;
        }
        if (verdi < min)
        {
            Feil($"Må være minst {min}.");
            continue;
        }
        if (max.HasValue && verdi > max.Value)
        {
            Feil($"Må være mellom {min} og {max.Value}.");
            continue;
        }
        return verdi;
    }
}
