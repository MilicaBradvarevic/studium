using Domain;

namespace Application;

public sealed record KreirajTimskiZadatakZahtev(
    string ZadatakId,
    string TimId,
    Student AutorSlanja,
    List<Student> TrenutniClanoviTima,
    string Sadrzaj,
    int? ProjektnaFazaId = null);

public sealed record PredajTimskiRadZahtev(
    Guid TimId,
    string KorisnikId,
    string Sadrzaj);

public sealed record PrihvatiPredajuZahtev(
    Guid NastavnikId);

public sealed record KreirajProjektnuFazuZahtev(
    Guid ProjekatId,
    string NazivProjekta,
    string NazivFaze,
    int RedosledFaze,
    string OcekivaniRezultat,
    DateTime Rok,
    string PovezaniZadatakId,
    int? PrethodnaFazaId);

public sealed record TimskaPredajaDto(
    Guid Id,
    Guid TimskiZadatakId,
    Guid TimId,
    Guid AutorSlanjaId,
    string Sadrzaj,
    DateTime VremePredajeUtc,
    bool JePrihvacena,
    Guid? PrihvatioNastavnikId,
    DateTime? VremePrihvatanjaUtc,
    IReadOnlyList<ClanTimaSnapshot> SnimakClanstvaPriPredaji);

public sealed record PregledFazeTimaDto(
    int FazaId,
    string NazivProjekta,
    string NazivFaze,
    int RedosledFaze,
    string OcekivaniRezultat,
    DateTime Rok,
    string PovezaniZadatakId,
    int? PrethodnaFazaId,
    bool PrethodnaFazaPrihvacena,
    string Status,
    bool Zavrsena);

public sealed class TimskiZadaciIFazeServis(ITimskiZadaciIFazeRepozitorijum repozitorijum)
{
    private readonly ITimskiZadaciIFazeRepozitorijum _repozitorijum = repozitorijum;

    public async Task<TimskiZadatak> KreirajZadatakAsync(
        KreirajTimskiZadatakZahtev zahtev,
        CancellationToken cancellationToken)
    {
        var zadatak = new TimskiZadatak(
            zahtev.ZadatakId,
            zahtev.TimId,
            zahtev.AutorSlanja,
            zahtev.TrenutniClanoviTima,
            zahtev.Sadrzaj);

        if (zahtev.ProjektnaFazaId is int fazaId)
        {
            zadatak.ProjektnaFazaId = fazaId;
        }

        await _repozitorijum.SacuvajZadatakAsync(zadatak, cancellationToken);
        return zadatak;
    }

    public async Task<TimskaPredajaDto> PredajIliZameniRadAsync(
        Guid zadatakId,
        PredajTimskiRadZahtev zahtev,
        CancellationToken cancellationToken)
    {
        var zadatak = await _repozitorijum.VratiZadatakPoIdAsync(zadatakId, cancellationToken)
            ?? throw new KeyNotFoundException("Timski zadatak nije pronađen.");

        var tim = await _repozitorijum.VratiTimPoIdAsync(zahtev.TimId, cancellationToken)
            ?? throw new KeyNotFoundException("Tim nije pronađen.");

        if (!string.Equals(zadatak.TimId, zahtev.TimId.ToString(), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Tim ne pripada ovom zadatku.");
        }

        var sadaUtc = DateTime.UtcNow;
        if (zadatak.ProjektnaFaza is not null && sadaUtc > zadatak.ProjektnaFaza.Rok)
        {
            throw new InvalidOperationException("Rok za predaju timskog zadatka je istekao.");
        }

        zadatak.PredajRad(zahtev.Sadrzaj);

        var snapshot = tim.NapraviSnapshotClanstva();
        var postojecaPredaja = await _repozitorijum.VratiTekucuPredajuAsync(zadatakId, zahtev.TimId, cancellationToken);

        if (postojecaPredaja is null)
        {
            postojecaPredaja = new TimskaPredaja(
                Guid.NewGuid(),
                zadatakId,
                zahtev.TimId,
                Guid.Parse(zahtev.KorisnikId),
                zahtev.Sadrzaj,
                sadaUtc,
                snapshot);
        }
        else
        {
            postojecaPredaja.ZameniRad(
                Guid.Parse(zahtev.KorisnikId),
                zahtev.Sadrzaj,
                sadaUtc,
                snapshot);
        }

        await _repozitorijum.SacuvajPredajuAsync(postojecaPredaja, cancellationToken);
        return MapirajPredaju(postojecaPredaja);
    }

    public async Task<TimskaPredajaDto?> VratiTekucuPredajuZaClanaAsync(
        Guid zadatakId,
        Guid timId,
        Guid korisnikId,
        CancellationToken cancellationToken)
    {
        var predaja = await _repozitorijum.VratiTekucuPredajuAsync(zadatakId, timId, cancellationToken);
        if (predaja is null)
        {
            return null;
        }

        if (!predaja.MozePregledati(korisnikId))
        {
            throw new UnauthorizedAccessException("Korisnik nije obuhvaćen ovom predajom.");
        }

        return MapirajPredaju(predaja);
    }

    public async Task<TimskaPredajaDto> PrihvatiPredajuAsync(
        Guid predajaId,
        PrihvatiPredajuZahtev zahtev,
        CancellationToken cancellationToken)
    {
        var predaja = await _repozitorijum.VratiPredajuPoIdAsync(predajaId, cancellationToken)
            ?? throw new KeyNotFoundException("Predaja nije pronađena.");

        if (predaja.JePrihvacena)
        {
            throw new InvalidOperationException("Predaja je već prihvaćena.");
        }

        predaja.Prihvati(zahtev.NastavnikId, DateTime.UtcNow);
        await _repozitorijum.SacuvajPredajuAsync(predaja, cancellationToken);

        return MapirajPredaju(predaja);
    }

    public async Task<ProjektnaFaza> KreirajFazuAsync(
        KreirajProjektnuFazuZahtev zahtev,
        CancellationToken cancellationToken)
    {
        var postojeceFaze = await _repozitorijum.VratiFazeProjektaAsync(zahtev.ProjekatId, cancellationToken);
        if (postojeceFaze.Any(f => f.RedosledFaze == zahtev.RedosledFaze))
        {
            throw new InvalidOperationException($"Faza sa redosledom {zahtev.RedosledFaze} već postoji u projektu.");
        }

        ProjektnaFaza? prethodnaFaza = null;
        if (zahtev.PrethodnaFazaId.HasValue)
        {
            prethodnaFaza = postojeceFaze.FirstOrDefault(f => f.Id == zahtev.PrethodnaFazaId.Value)
                ?? throw new KeyNotFoundException("Prethodna faza nije pronađena.");
        }

        if (Guid.TryParse(zahtev.PovezaniZadatakId, out var povezaniZadatakGuid))
        {
            _ = await _repozitorijum.VratiZadatakPoIdAsync(povezaniZadatakGuid, cancellationToken);
        }

        var faza = new ProjektnaFaza(
            zahtev.NazivProjekta,
            zahtev.NazivFaze,
            zahtev.RedosledFaze,
            zahtev.OcekivaniRezultat,
            zahtev.Rok,
            zahtev.PovezaniZadatakId,
            prethodnaFaza);

        await _repozitorijum.SacuvajFazuAsync(faza, cancellationToken);
        return faza;
    }

    public async Task<IReadOnlyList<PregledFazeTimaDto>> VratiPregledFazaPoTimuAsync(
        Guid projekatId,
        string nazivProjekta,
        Guid timId,
        CancellationToken cancellationToken)
    {
        _ = await _repozitorijum.VratiTimPoIdAsync(timId, cancellationToken)
            ?? throw new KeyNotFoundException("Tim nije pronađen.");

        var faze = (await _repozitorijum.VratiFazeProjektaAsync(projekatId, cancellationToken))
            .OrderBy(f => f.RedosledFaze)
            .ToList();

        var fazePoId = faze.ToDictionary(f => f.Id);
        List<PregledFazeTimaDto> rezultat = new(faze.Count);

        foreach (var faza in faze)
        {
            if (Guid.TryParse(faza.PovezaniZadatakId, out var povezaniZadatakGuid))
            {
                _ = await _repozitorijum.VratiTekucuPredajuAsync(povezaniZadatakGuid, timId, cancellationToken);
            }

            bool prethodnaPrihvacena = true;
            if (faza.PrethodnaFazaId.HasValue && fazePoId.TryGetValue(faza.PrethodnaFazaId.Value, out var prethodna))
            {
                prethodnaPrihvacena = prethodna.JeZatvorena;
            }

            var zavrsena = faza.JeZatvorena;
            var status = zavrsena ? "Zatvorena" : "Aktivna";

            rezultat.Add(new PregledFazeTimaDto(
                faza.Id,
                nazivProjekta,
                faza.NazivFaze,
                faza.RedosledFaze,
                faza.OcekivaniRezultat,
                faza.Rok,
                faza.PovezaniZadatakId,
                faza.PrethodnaFazaId,
                prethodnaPrihvacena,
                status,
                zavrsena));
        }

        return rezultat;
    }

    private static TimskaPredajaDto MapirajPredaju(TimskaPredaja p) =>
        new(
            p.Id,
            p.TimskiZadatakId,
            p.TimId,
            p.AutorSlanjaId,
            p.Sadrzaj,
            p.VremePredajeUtc,
            p.JePrihvacena,
            p.PrihvatioNastavnikId,
            p.VremePrihvatanjaUtc,
            p.SnimakClanstvaPriPredaji);
}
