namespace UniversitetsSystem
{
    // Abstrakt klasse, kan ikke instansieres direkte. Tvinger subclasses til å implementere abstrakte metoder
    public abstract class Bruker
    {
        // auto-implementerte egenskaper 
        // private set kan bare settes inni klassen
        public string Navn { get; private set; }
        public string Epost {get; private set; }

        // konstruktør, alle subklasser kaller denne via base
        protected Bruker(string navn, string epost)
        {
            Navn = navn;
            Epost = epost;
        }

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