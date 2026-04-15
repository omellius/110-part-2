namespace UniversitetsSystem
{
    // utvekslingsstudent arver fra Student, som arver fra Bruker
    public class Utvekslingsstudent : Student
    {
        public string Hjemuniversitet { get; private set; }
        public string Land { get; private set; }
        public DateTime PeriodeFra { get; private set; }
        public DateTime PeriodeTil { get; private set; }

        public Utvekslingsstudent(
            string StudentID,
            string navn,
            string epost,
            string brukernavn, 
            string passord,
            string hjemuniversitet,
            string land,
            DateTime periodeFra,
            DateTime periodeTil)
            : base(StudentID, navn, epost, brukernavn, passord)
        {
            Hjemuniversitet = hjemuniversitet;
            Land = land;
            PeriodeFra = periodeFra;
            PeriodeTil = periodeTil;
        }

        //  override igjen. utvider med spesifike detaljer
        public override string HentInfo()
        {
            return $"{base.HentInfo()} | Utveksling fra: {Hjemuniversitet} ({Land}) | Periode: {PeriodeFra:dd.MM.yyyy} - {PeriodeTil:dd.MM.yyyy}";
        }
        
    }
}