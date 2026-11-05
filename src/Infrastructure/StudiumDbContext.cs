using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure;

public class StudiumDbContext : DbContext
{
    public DbSet<ProjektnaFaza> ProjektneFaze { get; set; } = null!;
    public DbSet<TimskiZadatak> TimskiZadaci { get; set; } = null!;

    public StudiumDbContext(DbContextOptions<StudiumDbContext> options) : base(options)
    {
    }

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
