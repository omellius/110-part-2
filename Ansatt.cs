namespace UniversitetsSystem
{
    // arver også fra bruker
    public class Ansatt : Bruker
    {
        public string AnsattID { get; private set; }
        public string Stilling { get; private set; }
        public string Avdeling { get; private set; }

        // NEW: Ansatt tar nå også rolle + innloggingsdata.
        // Årsak: samme ansatt-klasse brukes for både faglærer og bibliotekansatt.
        public Ansatt(string ansattID, string navn, string epost, string brukernavn, string passord, string stilling, string avdeling, BrukerRolle rolle)
            : base(navn, epost, brukernavn, passord, rolle)
        {
            AnsattID = ansattID;
            Stilling = stilling;
            Avdeling = avdeling;
        }

        //override på hentinfo igjen
        public override string HentInfo()
        {
            return $"[Ansatt] ID: {AnsattID} | Navn: {Navn} | Stilling: {Stilling} | Avdeling: {Avdeling}";
        }
    }
}