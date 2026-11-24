using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public sealed record ClanTimaSnapshot(Guid KorisnikId, string ImePrezime);

public sealed class Tim
{
    private readonly List<ClanTimaSnapshot> _clanovi = [];

    public Guid Id { get; init; }
    public Guid ProjekatId { get; init; }
    public string Naziv { get; private set; }
    public IReadOnlyList<ClanTimaSnapshot> Clanovi => _clanovi.AsReadOnly();

    public Tim(Guid id, Guid projekatId, string naziv, IEnumerable<ClanTimaSnapshot> clanovi)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID tima je obavezan.", nameof(id));
        }
        if (projekatId == Guid.Empty)
        {
            throw new ArgumentException("ID projekta je obavezan.", nameof(projekatId));
        }
        if (string.IsNullOrWhiteSpace(naziv))
        {
            throw new ArgumentException("Naziv tima je obavezan.", nameof(naziv));
        }
        var lista = clanovi?.ToList() ?? [];
        if (lista.Count == 0)
        {
            throw new ArgumentException("Tim mora imati bar jednog člana.", nameof(clanovi));
        }

        Id = id;
        ProjekatId = projekatId;
        Naziv = naziv.Trim();
        _clanovi.AddRange(lista.DistinctBy(c => c.KorisnikId));
    }

    public bool JeTrenutniClan(Guid korisnikId) => _clanovi.Any(c => c.KorisnikId == korisnikId);


    public List<ClanTimaSnapshot> NapraviSnapshotClanstva()
    {
        return [.. _clanovi.Select(c => new ClanTimaSnapshot(c.KorisnikId, c.ImePrezime))];
    }

    public void DodajClana(ClanTimaSnapshot clan)
    {
        if (_clanovi.Any(c => c.KorisnikId == clan.KorisnikId))
        {
            throw new InvalidOperationException("Korisnik je već član tima.");
        }
        _clanovi.Add(clan);
    }

    public void UkloniClana(Guid korisnikId)
    {
        var clan = _clanovi.FirstOrDefault(c => c.KorisnikId == korisnikId)
            ?? throw new KeyNotFoundException("Korisnik nije član tima.");

        if (_clanovi.Count == 1)
        {
            throw new InvalidOperationException("Tim ne može ostati bez članova.");
        }

        _clanovi.Remove(clan);

    }
}
