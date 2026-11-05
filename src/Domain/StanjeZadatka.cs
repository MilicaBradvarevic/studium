using Xunit;
using Domain;
using System;
using System.Collections.Generic;

namespace Tests;

public class FazaIZadaciTestovi
{
    [Fact]
    public void Faza_Se_Ne_Moze_Zatvoriti_Ako_Zadaci_Nisu_Prihvaceni()
    {
        var faza = new ProjektnaFaza(
            "Naziv Projekta",
            "Faza 1",
            1,
            "Očekivani rezultat",
            DateTime.Now.AddDays(10),
            "ZadatakID1"
        );

        var zadatak = new TimskiZadatak(
            "Zadatak 1",
            "Tim1",
            "Ana",
            new List<string> { "Ana", "Miloš" },
            "Početni sadržaj"
        );

        faza.DodajZadatak(zadatak);

        Assert.Throws<InvalidOperationException>(() => faza.ZatvoriFazu());
    }

    [Fact]
    public void Zadatak_Nakon_Predaje_Menja_Stanje_U_Predato()
    {
        var zadatak = new TimskiZadatak(
            "Zadatak 2",
            "Tim1",
            "Miloš",
            new List<string> { "Miloš" },
            "Početni sadržaj"
        );

        zadatak.PredajRad("Kod je završen.");

        Assert.Equal(StanjeZadatka.Predato, zadatak.Status);
        Assert.NotNull(zadatak.SadrzajPredaje);
    }
}
