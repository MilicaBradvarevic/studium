using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class StudiumDbContext(DbContextOptions<StudiumDbContext> options) : DbContext(options)
{
    public DbSet<ProjektnaFaza> ProjektneFaze { get; set; } = null!;
    public DbSet<TimskiZadatak> TimskiZadaci { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjektnaFaza>()
            .HasMany(f => f.Zadaci)
            .WithOne(z => z.ProjektnaFaza)
            .HasForeignKey(z => z.ProjektnaFazaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
