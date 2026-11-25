using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public sealed class TimskaPredaja
{
    private readonly List<ClanTimaSnapshot> _snimakClanstvaPriPredaji = [];

    public Guid Id { get; init; }
    public Guid TimskiZadatakId { get; init; }
    public Guid TimId { get; init; }
    public Guid AutorSlanjaId { get; private set; }
    public string Sadrzaj { get; private set; }
    public DateTime VremePredajeUtc { get; private set; }
    public bool JePrihvacena { get; private set; }
    public Guid? PrihvatioNastavnikId { get; private set; }
    public DateTime? VremePrihvatanjaUtc { get; private set; }
    public IReadOnlyList<ClanTimaSnapshot> SnimakClanstvaPriPredaji => _snimakClanstvaPriPredaji.AsReadOnly();

    public TimskaPredaja(
        Guid id,
        Guid timskiZadatakId,
        Guid timId,
        Guid autorSlanjaId,
        string sadrzaj,
        DateTime vremePredajeUtc,
        IEnumerable<ClanTimaSnapshot> snimakClanstvaPriPredaji,
        bool jePrihvacena = false,
        Guid? prihvatioNastavnikId = null,
        DateTime? vremePrihvatanjaUtc = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID predaje je obavezan.", nameof(id));
        }
        if (timskiZadatakId == Guid.Empty)
        {
            throw new ArgumentException("ID timskog zadatka je obavezan.", nameof(timskiZadatakId));
        }
        if (timId == Guid.Empty)
        {
            throw new ArgumentException("ID tima je obavezan.", nameof(timId));
        }
        if (autorSlanjaId == Guid.Empty)
        {
            throw new ArgumentException("Autor slanja je obavezan.", nameof(autorSlanjaId));
        }
        if (string.IsNullOrWhiteSpace(sadrzaj))
        {
            throw new ArgumentException("Sadržaj predaje je obavezan.", nameof(sadrzaj));
        }

        var snapshot = snimakClanstvaPriPredaji?.ToList() ?? [];
        if (snapshot.Count == 0)
        {
            throw new ArgumentException("Predaja mora sadržati snimak članstva pri predaji.", nameof(snimakClanstvaPriPredaji));
        }
        Id = id;
        TimskiZadatakId = timskiZadatakId;
        TimId = timId;
        AutorSlanjaId = autorSlanjaId;
        Sadrzaj = sadrzaj.Trim();
        VremePredajeUtc = vremePredajeUtc;
        _snimakClanstvaPriPredaji.AddRange(snapshot.Select(c => new ClanTimaSnapshot(c.KorisnikId, c.ImePrezime)));
        JePrihvacena = jePrihvacena;
        PrihvatioNastavnikId = prihvatioNastavnikId;
        VremePrihvatanjaUtc = vremePrihvatanjaUtc;
    }

    public void ZameniRad(
        Guid noviAutorSlanjaId,
        string noviSadrzaj,
        DateTime vremeZameneUtc,
        IEnumerable<ClanTimaSnapshot> aktuelniSnapshotClanstva)
    {
        if (JePrihvacena)
        {
            throw new InvalidOperationException("Prihvaćena predaja ne može biti zamenjena.");
        }
        if (noviAutorSlanjaId == Guid.Empty)
        {
            throw new ArgumentException("Autor slanja je obavezan.", nameof(noviAutorSlanjaId));
        }
        if (string.IsNullOrWhiteSpace(noviSadrzaj))
        {
            throw new ArgumentException("Novi sadržaj predaje je obavezan.", nameof(noviSadrzaj));
        }
        var snapshot = aktuelniSnapshotClanstva?.ToList() ?? [];
        if (snapshot.Count == 0)
        {
            throw new ArgumentException("Snimak članstva pri zameni rada je obavezan.", nameof(aktuelniSnapshotClanstva));
        }
        AutorSlanjaId = noviAutorSlanjaId;
        Sadrzaj = noviSadrzaj.Trim();
        VremePredajeUtc = vremeZameneUtc;

        _snimakClanstvaPriPredaji.Clear();
        _snimakClanstvaPriPredaji.AddRange(snapshot.Select(c => new ClanTimaSnapshot(c.KorisnikId, c.ImePrezime)));
    }

    public void Prihvati(Guid nastavnikId, DateTime vremePrihvatanjaUtc)
    {
        if (nastavnikId == Guid.Empty)
        {
            throw new ArgumentException("ID nastavnika je obavezan.", nameof(nastavnikId));
        }
        JePrihvacena = true;
        PrihvatioNastavnikId = nastavnikId;
        VremePrihvatanjaUtc = vremePrihvatanjaUtc;
    }

    // Svi članovi obuhvaćeni predajom imaju pristup potvrdi i pregledu (ST-023)
    public bool MozePregledati(Guid korisnikId) =>
        _snimakClanstvaPriPredaji.Any(c => c.KorisnikId == korisnikId);
}
