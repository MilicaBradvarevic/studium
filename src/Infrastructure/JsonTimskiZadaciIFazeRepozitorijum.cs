using System.Text.Json;
using Application;
using Domain;

namespace Infrastructure;

public sealed class JsonTimskiZadaciIFazeRepozitorijum : ITimskiZadaciIFazeRepozitorijum
{
    private readonly string _putanjaFajla;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOpcije = new() { WriteIndented = true };

    public JsonTimskiZadaciIFazeRepozitorijum(string folderPodataka)
    {
        Directory.CreateDirectory(folderPodataka);
        _putanjaFajla = Path.Combine(folderPodataka, "st023_st024.json");
    }

    public async Task<Tim?> VratiTimPoIdAsync(Guid timId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            var r = skladiste.Timovi.FirstOrDefault(t => t.Id == timId);
            return r is null ? null : new Tim(r.Id, r.ProjekatId, r.Naziv, r.Clanovi);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SacuvajTimAsync(Tim tim, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            skladiste.Timovi.RemoveAll(t => t.Id == tim.Id);
            skladiste.Timovi.Add(new TimZapis(tim.Id, tim.ProjekatId, tim.Naziv, tim.Clanovi.ToList()));
            await SacuvajAsync(skladiste, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<TimskiZadatak?> VratiZadatakPoIdAsync(Guid zadatakId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            var r = skladiste.Zadaci.FirstOrDefault(z => z.GuidId == zadatakId);
            return r is null ? null : MapirajZadatak(r);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SacuvajZadatakAsync(TimskiZadatak zadatak, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            var guidId = Guid.TryParse(zadatak.ZadatakId, out var parsed) ? parsed : Guid.NewGuid();
            skladiste.Zadaci.RemoveAll(z => z.Id == zadatak.Id || z.GuidId == guidId);
            skladiste.Zadaci.Add(new ZadatakZapis(
                zadatak.Id,
                guidId,
                zadatak.ProjektnaFazaId,
                zadatak.ZadatakId,
                zadatak.TimId,
                zadatak.AutorSlanja,
                zadatak.SastavTimaUTrenutkuPredaje.ToList(),
                zadatak.Sadrzaj,
                zadatak.Status,
                zadatak.SadrzajPredaje,
                zadatak.VremePredaje));
            await SacuvajAsync(skladiste, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<TimskaPredaja?> VratiTekucuPredajuAsync(Guid zadatakId, Guid timId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            var r = skladiste.Predaje.FirstOrDefault(p => p.TimskiZadatakId == zadatakId && p.TimId == timId);
            return r is null ? null : MapirajPredaju(r);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<TimskaPredaja?> VratiPredajuPoIdAsync(Guid predajaId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            var r = skladiste.Predaje.FirstOrDefault(p => p.Id == predajaId);
            return r is null ? null : MapirajPredaju(r);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SacuvajPredajuAsync(TimskaPredaja predaja, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            skladiste.Predaje.RemoveAll(p => p.Id == predaja.Id || (p.TimskiZadatakId == predaja.TimskiZadatakId && p.TimId == predaja.TimId));
            skladiste.Predaje.Add(new PredajaZapis(
                predaja.Id,
                predaja.TimskiZadatakId,
                predaja.TimId,
                predaja.AutorSlanjaId,
                predaja.Sadrzaj,
                predaja.VremePredajeUtc,
                predaja.JePrihvacena,
                predaja.PrihvatioNastavnikId,
                predaja.VremePrihvatanjaUtc,
                predaja.SnimakClanstvaPriPredaji.ToList()));
            await SacuvajAsync(skladiste, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ProjektnaFaza?> VratiFazuPoIdAsync(Guid fazaId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            var r = skladiste.Faze.FirstOrDefault(f => f.GuidId == fazaId);
            return r is null ? null : MapirajFazu(r);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<ProjektnaFaza>> VratiFazeProjektaAsync(Guid projekatId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            return skladiste.Faze
                .Where(f => f.ProjekatId == projekatId)
                .Select(MapirajFazu)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SacuvajFazuAsync(ProjektnaFaza faza, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var skladiste = await UcitajAsync(cancellationToken);
            var guidId = Guid.NewGuid();
            var postojeci = skladiste.Faze.FirstOrDefault(x => x.Id == faza.Id && x.NazivProjekta == faza.NazivProjekta && x.RedosledFaze == faza.RedosledFaze);
            if (postojeci is not null)
            {
                guidId = postojeci.GuidId;
            }

            skladiste.Faze.RemoveAll(f => f.GuidId == guidId || (f.Id != 0 && f.Id == faza.Id));
            skladiste.Faze.Add(new FazaZapis(
                faza.Id,
                guidId,
                Guid.Empty,
                faza.NazivProjekta,
                faza.NazivFaze,
                faza.RedosledFaze,
                faza.OcekivaniRezultat,
                faza.Rok,
                faza.PovezaniZadatakId,
                faza.JeZatvorena,
                faza.PrethodnaFazaId));
            await SacuvajAsync(skladiste, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static TimskiZadatak MapirajZadatak(ZadatakZapis r)
    {
        var zadatak = new TimskiZadatak(
            r.ZadatakId,
            r.TimId,
            r.AutorSlanja,
            r.SastavTimaUTrenutkuPredaje,
            r.Sadrzaj)
        {
            Id = r.Id,
            ProjektnaFazaId = r.ProjektnaFazaId
        };

        if (!string.IsNullOrWhiteSpace(r.SadrzajPredaje))
        {
            zadatak.PredajRad(r.SadrzajPredaje);
        }

        if (r.Status == StanjeZadatka.Prihvaceno)
        {
            zadatak.PrihvatiZadatak();
        }

        return zadatak;
    }

    private static ProjektnaFaza MapirajFazu(FazaZapis r)
    {
        var faza = new ProjektnaFaza(
            r.NazivProjekta,
            r.NazivFaze,
            r.RedosledFaze,
            r.OcekivaniRezultat,
            r.Rok,
            r.PovezaniZadatakId)
        {
            Id = r.Id
        };

        if (r.JeZatvorena)
        {
            faza.ZatvoriFazu();
        }

        return faza;
    }

    private static TimskaPredaja MapirajPredaju(PredajaZapis r) =>
        new(
            r.Id,
            r.TimskiZadatakId,
            r.TimId,
            r.AutorSlanjaId,
            r.Sadrzaj,
            r.VremePredajeUtc,
            r.SnimakClanstvaPriPredaji,
            r.JePrihvacena,
            r.PrihvatioNastavnikId,
            r.VremePrihvatanjaUtc);

    private async Task<SkladistePodataka> UcitajAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_putanjaFajla))
        {
            return new SkladistePodataka();
        }

        var json = await File.ReadAllTextAsync(_putanjaFajla, cancellationToken);
        return JsonSerializer.Deserialize<SkladistePodataka>(json, JsonOpcije) ?? new SkladistePodataka();
    }

    private async Task SacuvajAsync(SkladistePodataka skladiste, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(skladiste, JsonOpcije);
        await File.WriteAllTextAsync(_putanjaFajla, json, cancellationToken);
    }

    private sealed class SkladistePodataka
    {
        public List<TimZapis> Timovi { get; set; } = [];
        public List<ZadatakZapis> Zadaci { get; set; } = [];
        public List<PredajaZapis> Predaje { get; set; } = [];
        public List<FazaZapis> Faze { get; set; } = [];
    }

    private sealed record TimZapis(Guid Id, Guid ProjekatId, string Naziv, List<ClanTimaSnapshot> Clanovi);

    private sealed record ZadatakZapis(
        int Id,
        Guid GuidId,
        int ProjektnaFazaId,
        string ZadatakId,
        string TimId,
        Student AutorSlanja,
        List<Student> SastavTimaUTrenutkuPredaje,
        string Sadrzaj,
        StanjeZadatka Status,
        string? SadrzajPredaje,
        DateTime VremePredaje);

    private sealed record PredajaZapis(
        Guid Id,
        Guid TimskiZadatakId,
        Guid TimId,
        Guid AutorSlanjaId,
        string Sadrzaj,
        DateTime VremePredajeUtc,
        bool JePrihvacena,
        Guid? PrihvatioNastavnikId,
        DateTime? VremePrihvatanjaUtc,
        List<ClanTimaSnapshot> SnimakClanstvaPriPredaji);

    private sealed record FazaZapis(
        int Id,
        Guid GuidId,
        Guid ProjekatId,
        string NazivProjekta,
        string NazivFaze,
        int RedosledFaze,
        string OcekivaniRezultat,
        DateTime Rok,
        string PovezaniZadatakId,
        bool JeZatvorena,
        int? PrethodnaFazaId);
}
