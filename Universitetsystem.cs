namespace UniversitetsSystem
{
    // UniversitetSystem er "hjernen" i programmet, håndterer alle data og operasjoner
    public class UniversitetSystem
    {
        //kolleksjoner, dictionary gir oppslag via nøkkel (ID/kode)
        // Vi bruker dictionary istedenfor list for rask henting av enkeltoppføringer
        // NEW: Eget oppslag for brukernavn -> ID gjør innlogging raskere og enkel

        private readonly Dictionary<string, Bruker> _brukere = new();
        private readonly Dictionary<string, string> _brukernavnTilId = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Kurs> _kurs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Bok> _bøker = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Lån> _lånHistorikk = new();


        public Bruker? HentBruker(string id) =>
            _brukere.TryGetValue(id, out var b) ? b : null;

        public bool BrukerFinnes(string id) => _brukere.ContainsKey(id);
        public bool StudentFinnes(string id) => HentBruker(id) is Student;
        public bool KursFinnes(string kode) => _kurs.ContainsKey(kode);
        public bool BokFinnes(string id) => _bøker.ContainsKey(id);
        public bool BrukernavnFinnes(string brukernavn) => _brukernavnTilId.ContainsKey(brukernavn.Trim());

        public (bool suksess, string melding) RegistrerNyBruker(
            BrukerRolle rolle,
            string id,
            string navn,
            string epost,
            string brukernavn,
            string passord)
        {
            // NEW: ID normaliseres for å unngå "S001" vs "s001" dupes
            id = id.Trim().ToUpperInvariant();
            brukernavn = brukernavn.Trim();
            if (_brukere.ContainsKey(id)) return (false, "ID finnes allerede.");
            if (_brukernavnTilId.ContainsKey(brukernavn)) return (false, "Brukernavn finnes allerede.");

            Bruker nyBruker = rolle switch
            {
                // Rolle bestemmer hvilken konkret brukertype som opprettes.
                BrukerRolle.Student => new Student(id, navn.Trim(), epost.Trim(), brukernavn, passord),
                BrukerRolle.Faglærer => new Ansatt(id, navn.Trim(), epost.Trim(), brukernavn, passord, "Faglærer", "Undervisning", Brukerrolle.Faglærer),
                BrukerRolle.BibliotekAnsatt => new Ansatt(id, navn.Trim(), epost.Trim(), brukernavn, passord, "Bibliotekar", "Bibliotek", BrukerRolle.BibliotekAnsatt),
                _ => throw new InvalidOperationException("Ukjent rolle.")
            };

            _brukere[id] = nyBruker;
            _brukernavnTilId[brukernavn] = id;
            return (true, $"{rolle} registrert med ID {id}.");
        }

        public (bool suksess, string melding, Bruker? bruker) LoggInn(string brukernavn, string passord)
        {
            // NEW: Autentisering skjer i to steg.. brukernavn finnes + matcher passord
            if (!_brukernavnTilId.TryGetValue(brukernavn.Trim(), out var id))
                return (false, "Ukjent brukernavn.", null);
            var bruker = _brukere[id];
            if (!bruker.VerifiserPassord(passord))
                return (false, "Feil passord.", null);
            return (true, $"Innlogget som {bruker.Navn} ({bruker.Rolle}).", bruker);
        }

        public bool OpprettKurs(string kode, string navn, int studiepoeng, int maksPlasser, string faglærerAnsattID)
        {
            kode = kode.Trim().ToUpperInvariant();
            navn = navn.Trim();
            // NEW: Kurs valideres både på kode og navn for å hindre dupes
            if (_kurs.ContainsKey(kode)) return false;
            if (_kurs.Values.Any(k => k.Navn.Equals(navn, StringComparison.OrdinalIgnoreCase))) return false;
            // Kun ansatte med faglærer rolle kan opprette kurs
            if (HentBruker(faglærerAnsattID) is not Ansatt a || a.Rolle != BrukerRolle.Faglærer) return false;
            _kurs[kode] = new Kurs(kode, navn, studiepoeng, maksPlasser, faglærerAnsattID);
            return true;
        }

        public (bool suksess, string melding) RegistrerPensum(string faglærerID, string kurskode, string pensumTekst)
        {
            // eierskap.. lærer må både ha riktig rolle og undervise kurset
            if (HentBruker(faglærerID) is not Ansatt a || a.Rolle != BrukerRolle.Faglærer)
                return (false, "Kun faglærer kan registrere pensum.");
            if (!_kurs.TryGetValue(kurskode, out var kurs))
                return (false, "Kurs finnes ikke.");
            if (!kurs.faglærerAnsattID.Equals(faglærerID, StringComparison.OrdinalIgnoreCase))
                return (false, "Du kan kun registrere pensum i kurs du underviser. ");
            return kurs.RegistrerPensum(pensumTekst)
                ? (true, "Pensum registrert.")
                : (false, "Pensum er ugyldig eller finnes allerede.");
        }

        public (bool suksess, string melding) SettKarakter(string faglærerID, string kurskode, string studentID, string karakter)
        {
            // Samme sikkerhetsmønster som pensum
            if (HentBruker(faglærerID) is not Ansatt a || a.Rolle != BrukerRolle.Faglærer)
                return (false, "Kun faglærer kan sette karakter. ");
            if (!_kurs.TryGetValue(kurskode, out var kurs))
                return (false, "Kurs finnes ikke. ");
            if (!kurs.faglærerAnsattID.Equals(faglærerID, StringComparison.OrdinalIgnoreCase))
                return (false, "Du kan kun sette karakter i kurs du underviser.");
            return kurs.SettKarakter(studentID.Trim().ToUpperInvariant(), karakter)
                ? (true, "Karakter registrert.")
                : (false, "Studenten er ikke påmeldt kurset.");
        }

        public List<(string kurskode, string kursnavn, string? karakter)> HentStudentKarakter(string studentID)
        {
            // Returnerer både kursinfo og eventuell karakter i samme respons
            studentID = studentID.Trim().ToUpperInvariant();
            return _kurs.Values
                .Where(k => k.Påmeldte.Any(s => s.StudentID.Equals(studentID, StringComparison.OrdinalIgnoreCase)))
                .Select(k => (k.Kode, k.Navn, k.HentKarakterForStudent(studentID)))
                .ToList();
        }

        public List<Kurs> HentKursForStudent(string studentID) =>
            _kurs.Values.Where(k => k.Påmeldte.Any(s => s.StudentID.Equals(studentID, StringComparison.OrdinalIgnoreCase))).ToList();

        public List<Kurs> HentKursForFaglærer(string faglærerID) =>
            _kurs.Values.Where(k => k.faglærerAnsattID.Equals(faglærerID, StringComparison.OrdinalIgnoreCase)).ToList();


        public (bool suksess, string melding) MeldStudentPåKurs(string studentID, string kurskode)
        {
            var bruker = HentBruker(studentID);

            // is-operator: Sjekker om bruker er av type Student
            if (bruker is not Student student)
                return (false, "Finner ikke student med den ID-en.");

            if (!_kurs.TryGetValue(kurskode, out var kurs))
                return (false, "Finner ikke kurs med den koden.");

            return kurs.MeldPå(student)
                ? (true, $"{student.Navn} er meldt på {kurs.Navn}.")
                : (false, "Kurset er fullt eller studenten er allerede påmeldt.");
        }

        // Legger til meldstudentavkurs
        public (bool suksess, string melding) MeldStudentAvKurs(string studentID, string kurskode)
        {
            var bruker = HentBruker(studentID);

            if (bruker is not Student student)
            return (false, "Finner ikke student med den ID-en.");

            if (!_kurs.TryGetValue(kurskode, out var kurs))
            return (false, "Finner ikke kurs med den koden.");

            return kurs.MeldAv(student)
            ? (true, $"{student.Navn} er meldt av {kurs.Navn}.") 
            : (false, "Studenten er ikke påmeldt dette kurset.");
        }

        public void PrintAlleKurs()
        {
            if (!_kurs.Any())
            {
                Console.WriteLine(" Ingen kurs registrert.");
                return;
            }
            foreach (var kurs in _kurs.Values) kurs.PrintDetaljer();
        }

        // LINQ spørring: søker etter kurs basert på fritekst 
        // .Where() filtrerer - også bruker vi ISøkbar-interfacet som er polymorfisme
        public List<Kurs> SøkKurs(string søkeord) =>
            _kurs.Values.Where(k => k.Matcher(søkeord)).ToList();
           
           
            //------------------
            // Bibliotek-Metoder
            //------------------

            public bool RegistrerBok(string id, string tittel, string forfatter, int år, int antall)
            {
                id = id.Trim().ToUpperInvariant();
                if (_bøker.ContainsKey(id)) return false;
                _bøker[id] = new Bok(id, tittel.Trim(), forfatter.Trim(), år, antall);
                return true; 
            }

            public List<Bok> SøkBok(string søkeord) =>
                _bøker.Values.Where(b => b.Matcher(søkeord)).ToList();
            

            public (bool suksess, string melding) LånBok(string brukerID, string bokID)
            {
                var bruker = HentBruker(brukerID);
                if (bruker == null) return (false, "Finner ikke bruker.");

                if (!_bøker.TryGetValue(bokID, out var bok))
                    return (false, "Finner ikke bok.");

                // if-betingelser: blokkerer utlån hvis ingen tilgjengelige

                if (bok.Tilgjengelige == 0)
                    return (false, $"Ingen ledige eksemplarer av \"{bok.Tittel}\".");

                // Sjekk om bruker allerede låner denne boken
                bool harAktivtLån = _lånHistorikk.Any(l => l.ErAktivt && l.Låntaker == bruker && l.Bok == bok); 
                if (harAktivtLån) return (false, "Bruker har allerede lånt denne boken.");

                bok.LånUt();
                _lånHistorikk.Add(new Lån(bruker, bok));
                return (true, $"\"{bok.Tittel}\" er lånt ut til {bruker.Navn}.");
            }

            public (bool suksess, string melding) ReturnerBok(string brukerID, string bokID)
            {
                var bruker = HentBruker(brukerID);
                if (bruker == null) return (false, "Finner ikke bruker.");

                // Linq: firstordefault returnerer første match eller null
                var lån = _lånHistorikk.FirstOrDefault(
                    l => l.ErAktivt && l.Låntaker == bruker && l.Bok.BokID == bokID);

                if (lån == null) return (false, "Finner ikke aktivt lån.");

                lån.Returner();
                lån.Bok.LeverInn();
                return (true, $"\"{lån.Bok.Tittel}\" er returnert");  
            }

            //to nye funksjoner

            public List<Lån> HentAktiveLån() => _lånHistorikk.Where(l => l.ErAktivt).ToList();
            public List<Lån> HentLånHistorikk() => _lånHistorikk.ToList();

            public void PrintAktiveLån()
            {
                // linq filtrerer kun aktive lån
                var aktive = _lånHistorikk.Where(l => l.ErAktivt).ToList();
                if (!aktive.Any()) 
                {
                    Console.WriteLine("  Ingen aktive lån.");
                    return;
                }

                foreach (var lån in aktive) Console.WriteLine($"  {lån}");
            }

            public void PrintLånHistorikk()
            {
                var historikk = HentLånHistorikk
                if (!historikk.Any())
                {
                    Console.WriteLine(" Ingen lån i historikk.");
                    return;
                }
                foreach (var lån in historikk) Console.WriteLine($" {lån}");
            }

            //Hjelpemetode, fyller med testdata slik at man kan teste raskt

            public void FyllTestData()
            {

                // Oppretter brukere, testdata inkluderer nå innlogginsbrukere også

                RegistrerNybruker(BrukerRolle.Student, "S001", "Ola Nordmann", "ola@uni.no", "ola123", "1234");
                RegistrerNyBruker(BrukerRolle.Student, "S002", "Maria Garcia", "maria@uni.es", "maria123", "4321");
                RegistrerNyBruker(BrukerRolle.Faglærer, "A001", "Kari Lærer", "kari@uni.no", "kari123", "5678");
                RegistrerNyBruker(BrukerRolle.BibliotekAnsatt, "A002", "Per Bibliotekar", "per@uni.no", "per123", "8765");

                // Oppretter kurs
                OpprettKurs("INF101", "Introduksjon til programmering", 10, 30);
                OpprettKurs("INF201", "Objektorientert programmering", 10, 25);
                OpprettKurs("MAT101", "Matematikk 1", 10, 40);

                // Melder på studenter
                MeldStudentPåKurs("S001", "INF101");
                MeldStudentPåKurs("S001", "INF201");
                MeldStudentPåKurs("S002", "INF101");

                // Registrerer bøker
                RegistrerBok("B001", "C# i praksis", "Jan Hansen", 2022, 3);
                RegistrerBok("B002", "Design Patterns", "Robert Martin", 1994, 2);
                RegistrerBok("B003", "Clean code", "John Johnson", 2008, 1);

                // Lager et lån
                LånBok("S001", "B001");
            }
        }
}