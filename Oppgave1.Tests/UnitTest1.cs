using UniversitetsSystem;

namespace Oppgave1.Tests;

public class UniversitetSystemTests
{
    [Fact]
    public void OpprettKurs_StopperDuplikatKodeOgNavn()
    {
        // NEW: Verifiserer forretningsregel om unike kurs (både kode og navn).
        var system = new UniversitetSystem();
        system.FyllTestData();

        bool duplikatKode = system.OpprettKurs("INF101", "Nytt navn", 10, 20, "A001");
        bool duplikatNavn = system.OpprettKurs("INF999", "Introduksjon til programmering", 10, 20, "A001");

        Assert.False(duplikatKode);
        Assert.False(duplikatNavn);
    }

    [Fact]
    public void MeldStudentPåKurs_StopperDobbelPåmelding()
    {
        // NEW: Verifiserer at samme student ikke kan meldes på samme kurs to ganger.
        var system = new UniversitetSystem();
        system.FyllTestData();

        var første = system.MeldStudentPåKurs("S002", "INF201");
        var andre = system.MeldStudentPåKurs("S002", "INF201");

        Assert.True(første.suksess);
        Assert.False(andre.suksess);
    }

    [Fact]
    public void SettKarakter_KreverRiktigFaglaerer()
    {
        // NEW: Verifiserer rolle- og eierskapskontroll ved karaktersetting.
        var system = new UniversitetSystem();
        system.FyllTestData();

        var feilLærer = system.SettKarakter("A002", "INF101", "S001", "A");
        var riktigLærer = system.SettKarakter("A001", "INF101", "S001", "A");

        Assert.False(feilLærer.suksess);
        Assert.True(riktigLærer.suksess);
    }

    [Fact]
    public void ReturnerBok_FlytterLånFraAktivTilHistorikk()
    {
        // NEW: Verifiserer at retur oppdaterer både aktive lån og historikk korrekt.
        var system = new UniversitetSystem();
        system.FyllTestData();

        var lån = system.LånBok("S001", "B002");
        var aktiveFør = system.HentAktiveLån().Count;
        var retur = system.ReturnerBok("S001", "B002");
        var aktiveEtter = system.HentAktiveLån().Count;
        var historikk = system.HentLånHistorikk();

        Assert.True(lån.suksess);
        Assert.True(retur.suksess);
        Assert.True(aktiveEtter < aktiveFør);
        Assert.Contains(historikk, l => !l.ErAktivt && l.Bok.BokID == "B002");
    }
}
