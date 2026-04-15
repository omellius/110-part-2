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

        //liste av studentobjekter som er meldt på kurset
        private List<Student> _påmeldte = new List<Student>();

        // Skrivebeskyttet tilgang utenfra, kan ikke endres direkte
        public IReadOnlyList<Student> Påmeldte => _påmeldte.AsReadOnly();

        public Kurs(string kode, string navn, int studiepoeng, int maksPlasser)
        {
            Kode = kode;
            Navn = navn;
            Studiepoeng = studiepoeng;
            MaksPlasser = maksPlasser;
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

            // loop som itererer mellom alle påmeldte studenter
            foreach (var student in _påmeldte)
            Console.WriteLine($"    - {student.Navn} ({student.StudentID})");
        }
    }
}