using Xunit;
using Domain;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

namespace Tests.Domain;

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

        Student ana = new Student("Ana", 1);
        Student milos = new Student("Miloš", 2);

        var zadatak = new TimskiZadatak(
            "Zadatak 1",
            "Tim1",
            ana,
            new List<Student> { ana, milos },
            "Početni sadržaj"
        );

        faza.DodajZadatak(zadatak);

        Assert.Throws<InvalidOperationException>(() => faza.ZatvoriFazu());
    }

    [Fact]
    public void Zadatak_Nakon_Predaje_Menja_Stanje_U_Predato()
    {
        Student milos = new Student("Miloš", 1);
        var zadatak = new TimskiZadatak(
            "Zadatak 2",
            "Tim1",
            milos,
            new List<Student> { milos },
            "Početni sadržaj"
        );

        zadatak.PredajRad("Kod je završen.");

        Assert.Equal(StanjeZadatka.Predato, zadatak.Status);
        Assert.NotNull(zadatak.SadrzajPredaje);
    }

    [Fact]
    public void Zadatak_Se_Moze_Prihvatiti_Tek_Nakon_Predaje_I_Tada_Se_Faza_Moze_Zatvoriti()
    {
        var faza = new ProjektnaFaza(
            "Naziv Projekta",
            "Faza 1",
            1,
            "Očekivani rezultat",
            DateTime.Now.AddDays(10),
            "ZadatakID1"
        );

        Student ana = new Student("Ana", 1);
        var zadatak = new TimskiZadatak(
            "Zadatak 1",
            "Tim1",
            ana,
            new List<Student> { ana },
            "Početni sadržaj"
        );

        faza.DodajZadatak(zadatak);
        zadatak.PredajRad("Finalno rešenje");
        zadatak.PrihvatiZadatak();

        Assert.Equal(StanjeZadatka.Prihvaceno, zadatak.Status);
        faza.ZatvoriFazu();
        Assert.True(faza.JeZatvorena);
    }

    [Fact]
    public void Prazan_Sadrzaj_Predaje_Baca_Izuzetak()
    {
        Student milos = new Student("Miloš", 1);
        var zadatak = new TimskiZadatak(
            "Zadatak 3",
            "Tim1",
            milos,
            new List<Student> { milos },
            "Početni sadržaj"
        );

        Assert.Throws<ArgumentException>(() => zadatak.PredajRad("   "));
    }
}
