using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain;
using Infrastructure;
using Xunit;

namespace Tests;

public sealed class JsonTimskiZadaciIFazeRepozitorijumTests
{
    [Fact]
    public async Task SacuvajTimAsync_I_VratiTimPoIdAsync_UpisujeIUcitavaIzJsona()
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "st023_st024_tests_" + Guid.NewGuid());

        try
        {
            var repo = new JsonTimskiZadaciIFazeRepozitorijum(tempFolder);
            var timId = Guid.NewGuid();
            var projekatId = Guid.NewGuid();
            var clan = new ClanTimaSnapshot(Guid.NewGuid(), "Test Clan");
            var tim = new Tim(timId, projekatId, "Tim Test", [clan]);

            await repo.SacuvajTimAsync(tim, CancellationToken.None);

            var ucitani = await repo.VratiTimPoIdAsync(timId, CancellationToken.None);
            var jsonFajl = Path.Combine(tempFolder, "st023_st024.json");

            Assert.True(File.Exists(jsonFajl));
            Assert.NotNull(ucitani);
            Assert.Equal("Tim Test", ucitani!.Naziv);
            Assert.Single(ucitani.Clanovi);
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, recursive: true);
            }
        }
    }
}
