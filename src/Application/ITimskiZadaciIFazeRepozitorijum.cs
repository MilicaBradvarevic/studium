using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application;                                                                                                                using Domain;

namespace Application;

public interface ITimskiZadaciIFazeRepozitorijum
{
    Task<Tim?> VratiTimPoIdAsync(Guid timId, CancellationToken cancellationToken);
    Task SacuvajTimAsync(Tim tim, CancellationToken cancellationToken);

    Task<TimskiZadatak?> VratiZadatakPoIdAsync(Guid zadatakId, CancellationToken cancellationToken);
    Task SacuvajZadatakAsync(TimskiZadatak zadatak, CancellationToken cancellationToken);

    Task<TimskaPredaja?> VratiTekucuPredajuAsync(Guid zadatakId, Guid timId, CancellationToken cancellationToken);
    Task<TimskaPredaja?> VratiPredajuPoIdAsync(Guid predajaId, CancellationToken cancellationToken);
    Task SacuvajPredajuAsync(TimskaPredaja predaja, CancellationToken cancellationToken);

    Task<ProjektnaFaza?> VratiFazuPoIdAsync(Guid fazaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProjektnaFaza>> VratiFazeProjektaAsync(Guid projekatId, CancellationToken cancellationToken);
    Task SacuvajFazuAsync(ProjektnaFaza faza, CancellationToken cancellationToken);
}
