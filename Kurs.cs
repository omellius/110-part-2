namespace UniversitetsSystem
{
    //interface, klasser som implementerer MÅ ha visse metoder
    // skiller seg fra abstrakt klasse, ingen feltvariabler, ingen konstruktør, bare metoder
    public interface ISøkbar
    {
        bool Matcher(string søkeord);
    }

    // Kurs implementerer interfaces ISøkbar
    public class Kurs : ISøkbar
    {
        public string Kode { get; private set; }
        public string Navn {get; private set; }
        public int Studiepoeng { get; private set; }
        public int MaksPlasser { get; private set; }
        // NEW: Knytter kurset til ansvarlig faglærer (brukes i autorisasjonssjekker).
        public string FaglaererAnsattID { get; private set; }

        //liste av studentobjekter som er meldt på kurset
        private List<Student> _påmeldte = new List<Student>();
        // NEW: Kurset holder selv pensumlisten.
        private List<string> _pensum = new List<string>();
        // NEW: Karakter lagres per studentID for dette kurset.
        private Dictionary<string, string> _karakterer = new Dictionary<string, string>();

        // Skrivebeskyttet tilgang utenfra, kan ikke endres direkte
        public IReadOnlyList<Student> Påmeldte => _påmeldte.AsReadOnly();

        public IReadOnlyList<string> Pensum => _pensum.AsReadOnly();

        public Kurs(string kode, string navn, int studiepoeng, int maksPlasser, string faglaererAnsattID)
        {
            Kode = kode;
            Navn = navn;
            Studiepoeng = studiepoeng;
            MaksPlasser = maksPlasser;
            FaglaererAnsattID = faglaererAnsattID;
        }

        public bool MeldPå(Student student)
        {
            //sjekker kapasitet OG at studenten ikke allerede er påmeldt
            if (_påmeldte.Count >= MaksPlasser || _påmeldte.Any(s => s.StudentID == student.StudentID))
            return false;

            _påmeldte.Add(student);
            student.MeldPåKurs(Kode);
            return true;
        }

        public bool RegistrerPensum(string pensumTekst)
        {
            // NEW: Avviser tom eller duplisert pensumtekst.
            if (string.IsNullOrWhiteSpace(pensumTekst)) return false;
            if (_pensum.Any(p => p.Equals(pensumTekst, StringComparison.OrdinalIgnoreCase))) return false;
            _pensum.Add(pensumTekst.Trim());
            return true;
        }

        public bool SettKarakter(string studentID, string karakter)
        {
            // NEW: Forhindrer karaktersetting på studenter som ikke er påmeldt.
            bool studentErPåmeldt = _påmeldte.Any(s => s.StudentID == studentID);
            if (!studentErPåmeldt) return false;
            _karakterer[studentID] = karakter.Trim().ToUpperInvariant();
            return true;
        }

        public string? HentKarakterForStudent(string studentID) =>
            _karakterer.TryGetValue(studentID, out var karakter) ? karakter : null;

        public bool MeldAv(Student student)
        {
            var funnet = _påmeldte.FirstOrDefault(s => s.StudentID == student.StudentID);
            if (funnet == null) return false;

            _påmeldte.Remove(funnet);
            funnet.MeldAvkurs(Kode);
            return true;
        }

        //interface metode. Søker på tvers av kode og navn, case-insensitive
        public bool Matcher(string søkeord)
        {
            return Kode.Contains(søkeord, StringComparison.OrdinalIgnoreCase)
                || Navn.Contains(søkeord, StringComparison.OrdinalIgnoreCase);
        }

        public void PrintDetaljer()
        {
            Console.WriteLine($"\n Kurs: [{Kode}] {Navn} | {Studiepoeng} stp | {_påmeldte.Count}/{MaksPlasser} plasser");
            Console.WriteLine($" Faglærer: {FaglaererAnsattID}");
            if (_pensum.Any())
                Console.WriteLine($" Pensum: {string.Join(", ", _pensum)}");

            // loop som itererer mellom alle påmeldte studenter
            foreach (var student in _påmeldte)
            Console.WriteLine($"    - {student.Navn} ({student.StudentID})");
        }
    }
}