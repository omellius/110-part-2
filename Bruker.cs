namespace UniversitetsSystem
{
    // NEW: Rolle-enum brukes i både innlogging og rollebaserte menyer.
    // Vi holder rollene sentralt her for å unngå "hardkodede" tekstverdier flere steder.
    public enum BrukerRolle
    {
        Student,
        Faglaerer,
        BibliotekAnsatt
    }

    // Abstrakt klasse, kan ikke instansieres direkte. Tvinger subclasses til å implementere abstrakte metoder
    public abstract class Bruker
    {
        // auto-implementerte egenskaper 
        // private set kan bare settes inni klassen
        public string Navn { get; private set; }
        public string Epost {get; private set; }
        // NEW: Brukernavn er lagt til for autentisering (login med brukernavn/passord).
        public string Brukernavn { get; private set; }
        // NEW: Rolle lagres på brukerobjektet for å styre hvilke menyer/funksjoner som vises.
        public BrukerRolle Rolle { get; private set; }
        // NEW: Passord lagres privat slik at det ikke kan leses direkte utenfra.
        private string Passord { get; set; }

        // NEW: Konstruktøren tar nå inn autentiserings- og rolledata
        // fordi alle brukertyper må kunne logges inn og autoriseres.
        // konstruktør, alle subklasser kaller denne via base
        protected Bruker(string navn, string epost, string brukernavn, string passord, BrukerRolle rolle)
        {
            Navn = navn;
            Epost = epost;
            Brukernavn = brukernavn;
            Passord = passord;
            Rolle = rolle;
        }

        // NEW: Egen verifiseringsmetode gjør at passord-sjekken holdes i domenelaget.
        public bool VerifiserPassord(string passord) => Passord == passord;

        // Abstrakt metode, ingen implementasjon her, subclasses MÅ override den.
        // Bruk av polymorfisme
        public abstract string HentInfo();

        //virtual metode: har en standard, men subklasser KAN override
        public virtual void PrintInfo()
        {
            Console.WriteLine(HentInfo());
        }
    }
}