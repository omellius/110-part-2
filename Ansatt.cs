namespace UniversitetsSystem
{
    // arver også fra bruker
    public class Ansatt : Bruker
    {
        public string AnsattID { get; private set; }
        public string Stilling { get; private set; }
        public string Avdeling { get; private set; }

        public Ansatt(string ansattID, string navn, string epost, string stilling, string avdeling) : base(navn, epost)
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