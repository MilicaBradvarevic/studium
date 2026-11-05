using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain;

public class ProjektnaFaza
{
    public ICollection<TimskiZadatak> Zadaci { get; set; } = new List<TimskiZadatak>();
    public int Id { get; set; }
    public string NazivProjekta { get; set; } = string.Empty;
    public string NazivFaze { get; private set; } = string.Empty;
    public int RedosledFaze { get; private set; }
    public string OcekivaniRezultat { get; private set; } = string.Empty;
    public DateTime Rok { get; private set; }
    public string PovezaniZadatakId { get; private set; } = string.Empty;

    public int? PrethodnaFazaId { get; private set; }
    public ProjektnaFaza? PrethodnaFaza { get; private set; }

    public ProjektnaFaza() { }

    public ProjektnaFaza(string nazivProjekta, string nazivFaze, int redosledFaze, string ocekivaniRezultat, DateTime rok, string povezaniZadatakId, ProjektnaFaza? prethodnaFaza = null)
    {
        if (redosledFaze <= 0)
        {
            throw new ArgumentException("Redosled faze mora biti pozitivan broj.");
        }

        if (prethodnaFaza != null && prethodnaFaza.RedosledFaze >= redosledFaze)
        {
            throw new InvalidOperationException("Zavisnost može pokazivati samo na raniju fazu istog projekta.");
        }

        NazivProjekta = nazivProjekta;
        NazivFaze = nazivFaze;
        RedosledFaze = redosledFaze;
        OcekivaniRezultat = ocekivaniRezultat;
        Rok = rok;
        PovezaniZadatakId = povezaniZadatakId;
        PrethodnaFaza = prethodnaFaza;
        PrethodnaFazaId = prethodnaFaza?.Id;
    }

    // Metoda koja je falila u testovima za dodavanje zadatka
    public void DodajZadatak(TimskiZadatak zadatak)
    {
        Zadaci.Add(zadatak);
        zadatak.ProjektnaFaza = this;
    }

    // Metoda koja proverava da li su svi zadaci prihvaćeni pre zatvaranja faze
    public void ZatvoriFazu()
    {
        foreach (var zadatak in Zadaci)
        {
            if (zadatak.Status != StanjeZadatka.Prihvaceno)
            {
                throw new InvalidOperationException("Faza se ne može zatvoriti ako svi zadaci nisu prihvaćeni.");
            }
        }
    }
}
