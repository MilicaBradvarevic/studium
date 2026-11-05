using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class TimskiZadatak
{
    public int ProjektnaFazaId { get; set; }
    public ProjektnaFaza ProjektnaFaza { get; set; } = null!;
    public int Id { get; set; }
    public string ZadatakId { get; private set; } = string.Empty;
    public string TimId { get; private set; } = string.Empty;
    public string AutorSlanja { get; private set; } = string.Empty;
    public List<string> SastavTimaUTrenutkuPredaje { get; private set; } = new List<string>(); // Čuva sastav u trenutku podnošenja
    public string Sadrzaj { get; private set; } = string.Empty;
    public DateTime VremePredaje { get; private set; }
    public StanjeZadatka Status { get; set; }

    public TimskiZadatak() { }

    public TimskiZadatak(string zadatakId, string timId, string autorSlanja, List<string> trenutniClanoviTima, string sadrzaj)
    {
        if (string.IsNullOrWhiteSpace(sadrzaj))
        {
            throw new ArgumentException("Sadržaj predaje ne može biti prazan.");
        }

        ZadatakId = zadatakId;
        TimId = timId;
        AutorSlanja = autorSlanja;
        SastavTimaUTrenutkuPredaje = new List<string>(trenutniClanoviTima); // Fiksiramo sastav tima
        Sadrzaj = sadrzaj;
        VremePredaje = DateTime.UtcNow;
        Status = StanjeZadatka.Predato;
    }

    public void AzurirajRad(string noviSadrzaj)
    {
        if (string.IsNullOrWhiteSpace(noviSadrzaj))
        {
            throw new ArgumentException("Sadržaj ne može biti prazan.");

        }
        Sadrzaj = noviSadrzaj;
        VremePredaje = DateTime.UtcNow;

    }
}

