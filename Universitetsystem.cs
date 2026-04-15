namespace UniversitetsSystem
{
    // UniversitetSystem er "hjernen" i programmet, håndterer alle data og operasjoner
    public class UniversitetSystem
    {
        //kolleksjoner, dictionary gir oppslag via nøkkel (ID/kode)
        // Vi bruker dictionary istedenfor list for rask henting av enkeltoppføringer

        private Dictionary<string, Bruker> _brukere = new();
        private Dictionary<string, Kurs> _kurs = new();
        private Dictionary<string, Bok> _bøker = new();
        private List<Lån> _lånHistorikk = new List<Lån>();

        //----------------
        // Bruker-metoder
        //----------------

        public void LeggTilBruker(Bruker bruker)
        {
            // Bruker pattern matching trygt - unngår InvalidCastException hvis ukjent type
            string? id = bruker switch
            {
                Student s  => s.StudentID,
                Ansatt a   => a.AnsattID,
                _          => null
            };
            if (id != null && !_brukere.ContainsKey(id))
                _brukere[id] = bruker;
        }

        public Bruker? HentBruker(string id) =>
            _brukere.TryGetValue(id, out var b) ? b : null;
        

        //--------------
        // Kurs-metoder
        //--------------

        public bool OpprettKurs(string kode, string navn, int studiepoeng, int maksPlasser)
        {
            if (_kurs.ContainsKey(kode)) return false;
            _kurs[kode] = new Kurs(kode, navn, studiepoeng, maksPlasser);
            return true;
        }

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

        public void PrintAlleKurs()
        {
            if (!_kurs.Any())
            {
                Console.WriteLine(" Ingen kurs registrert.");
                return;
            }

            foreach (var kurs in _kurs.Values)
            {
                Console.WriteLine($"Kurs: {kurs.Kode} - {kurs.Navn} ({kurs.Studiepoeng} studiepoeng, {kurs.Påmeldte.Count}/{kurs.MaksPlasser} plasser)");
                if (kurs.Påmeldte.Any())
                {
                    Console.WriteLine("  Deltakere:");
                    foreach (var student in kurs.Påmeldte)
                    {
                        Console.WriteLine($"    {student.StudentID} - {student.Navn}");
                    }
                }
                else
                {
                    Console.WriteLine("  Ingen deltakere.");
                }
                Console.WriteLine();
            }
        }

        // LINQ spørring: søker etter kurs basert på fritekst 
        // .Where() filtrerer - også bruker vi ISøkbar-interfacet som er polymorfisme
        public List<Kurs> SøkKurs(string søkeord)
        {
            return _kurs.Values
                .Where(k => k.Matcher(søkeord))
                .ToList();
        }

            //------------------
            // Bibliotek-Metoder
            //------------------

            public bool RegistrerBok(string id, string tittel, string forfatter, int år, int antall)
            {
                if (_bøker.ContainsKey(id)) return false;
                _bøker[id] = new Bok(id, tittel, forfatter, år, antall);
                return true;
            }

            public List<Bok> SøkBok(string søkeord)
            {
                // LINQ filtrerer bøker
                return _bøker.Values
                    .Where(b => b.Matcher(søkeord))
                    .ToList();
            }

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

            //Hjelpemetode, fyller med testdata slik at man kan teste raskt

            public void FyllTestData()
            {

                // Oppretter brukere
                var s1 = new Student("S001", "Ola Nordmann", "ola@uni.no");
                var s2 = new Utvekslingsstudent("S002", "Maria Garcia", "maria@uni.es", "Universidad de Madrid", "Spania", new DateTime(2026, 1, 15), new DateTime(2026, 6, 30));
                var a1 = new Ansatt("A001", "Kari Lærer", "kari@uni.no", "Foreleser", "Informatikk");

                LeggTilBruker(s1);
                LeggTilBruker(s2);
                LeggTilBruker(a1);

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