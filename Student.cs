namespace UniversitetsSystem
{
    // student arver fra bruker
    public class Student : Bruker
    {
        public string StudentID { get; private set; }

        // kolleksjon. Generisk liste, her lagrer vi kurskoder studenten er meldt på
        public List<string> PåmeldteKurs { get; private set; }

        // kaller konstruktøren til bruker
        // konstruktøren tar nå også inn brukernavn og passord

        public Student(string studentID, string navn, string epost, string brukernavn, string passord) : base(navn, epost, brukernavn, passord, BrukerRolle.Student)
        {
            StudentID = studentID;
            PåmeldteKurs = new List<string>();
        }

        public void MeldPåKurs(string kurskode)
        {
            if (!PåmeldteKurs.Contains(kurskode))
                PåmeldteKurs.Add(kurskode);
        }

        public void MeldAvkurs(string kurskode)
        {
            PåmeldteKurs.Remove(kurskode);
        }

        // overrider hentinfo

        public override string HentInfo()
        {
            return $"[Student] ID: {StudentID} | Navn: {Navn} | E-post: {Epost} | Kurs: {PåmeldteKurs.Count}";
        }
    }
}