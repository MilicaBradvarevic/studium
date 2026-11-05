using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;

namespace Domain;

public enum StanjeZadatka
{
    Nepredato,
    Predato,
    Prihvaceno
}

public class TimskiZadatak
{
    public int ProjektnaFazaId { get; set; }
    public ProjektnaFaza ProjektnaFaza { get; set; } = null!;
    public int Id { get; set; }
    public string ZadatakId { get; private set; } = string.Empty;
    public string TimId { get; private set; } = string.Empty;
    public string AutorSlanja { get; private set; } = string.Empty;
    public List<string> SastavTimaUTrenutkuPredaje { get; private set; } = new List<string>();
    public string Sadrzaj { get; private set; } = string.Empty;

    // Svojstva koja su tražena u testovima
    public StanjeZadatka Status { get; set; } = StanjeZadatka.Nepredato;
    public string? SadrzajPredaje { get; private set; }
    public DateTime VremePredaje { get; private set; }

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
        SastavTimaUTrenutkuPredaje = new List<string>(trenutniClanoviTima);
        Sadrzaj = sadrzaj;
        Status = StanjeZadatka.Nepredato;
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

    // Metoda koja je falila u testovima za predaju rada
    public void PredajRad(string sadrzajPredaje)
    {
        if (string.IsNullOrWhiteSpace(sadrzajPredaje))
        {
            throw new ArgumentException("Sadržaj predaje ne može biti prazan.");
        }

        SadrzajPredaje = sadrzajPredaje;
        Status = StanjeZadatka.Predato;
        VremePredaje = DateTime.UtcNow;
    }
}

