using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class ProjektnaFaza
{
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

        // Sprečavanje ciklusa i provera da zavisnost ide samo na raniju fazu[cite: 6]
        if (prethodnaFaza != null)
        {
            if (prethodnaFaza.RedosledFaze >= redosledFaze)
            {
                throw new InvalidOperationException("Zavisnost može pokazivati samo na raniju fazu istog projekta.");
            }
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
}
