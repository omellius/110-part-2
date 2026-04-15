namespace UniversitetsSystem
{
    // Representerer en bok eller medium i biblioteket
    public class Bok: ISøkbar
    {
        public string BokID { get; private set; }
        public string Tittel { get; private set; }
        public string Forfatter { get; private set; }
        public int År { get; private set; }
        public int AntallEksemplarer { get; private set; }

        // Variabel som holder styr på tilgjengelige eksemplarer
        private int _tilgjengelige; 
        public int Tilgjengelige => _tilgjengelige;

        public Bok(string bokID, string tittel, string forfatter, int år, int antallEksemplarer)
        {
            BokID = bokID;
            Tittel = tittel;
            Forfatter = forfatter;
            År = år; 
            AntallEksemplarer = antallEksemplarer;
            _tilgjengelige = antallEksemplarer;
        }

        public bool LånUt()
        {
            if (_tilgjengelige <= 0) return false;
            _tilgjengelige--;
            return true;
        }

        public void LeverInn()
        {
            if (_tilgjengelige < AntallEksemplarer)
            _tilgjengelige++;
        }

        //implementerer ISøkbar kontrakten
        public bool Matcher(string søkeord)
        {
            return Tittel.Contains(søkeord, StringComparison.OrdinalIgnoreCase)
                || Forfatter.Contains(søkeord, StringComparison.OrdinalIgnoreCase)
                || BokID.Contains(søkeord, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"[{BokID}] \"{Tittel}\" av {Forfatter} ({År}) | {_tilgjengelige}/{AntallEksemplarer} tilgjengelig";
        }
    }

    // representerer ett enkelt lån. knyttes til en bruker (typ ansatt eller student)
    public class Lån
    {
        public Bruker Låntaker { get; } 
        public Bok Bok { get; }
        public DateTime LåntDato { get; }
        public DateTime? ReturDato { get; private set; } 

        public bool ErAktivt => ReturDato == null;

        public Lån(Bruker låntaker, Bok bok)
        {
            Låntaker = låntaker;
            Bok = bok;
            LåntDato = DateTime.Now;
        }

        public void Returner()
        {
            ReturDato = DateTime.Now;
        }

        public override string ToString()
        {
            string status = ErAktivt ? "AKTIV" : $"Returnert {ReturDato:dd.MM.yyyy}";
            return $"{Låntaker.Navn} lånte \"{Bok.Tittel}\" den {LåntDato:dd.MM.yyyy} [{status}]";
        }
    }
}