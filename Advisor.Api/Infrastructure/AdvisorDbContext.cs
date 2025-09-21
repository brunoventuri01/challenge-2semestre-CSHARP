using Advisor.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Advisor.Api.Infrastructure;

public class AdvisorDbContext : DbContext
{
    public AdvisorDbContext(DbContextOptions<AdvisorDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Ativo> Ativos => Set<Ativo>();
    public DbSet<Carteira> Carteiras => Set<Carteira>();
    public DbSet<Posicao> Posicoes => Set<Posicao>();
    public DbSet<MacroVariavel> Macros => Set<MacroVariavel>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Posicao>()
          .HasOne(p => p.Carteira)
          .WithMany(c => c.Posicoes)
          .HasForeignKey(p => p.CarteiraId);

        mb.Entity<Posicao>()
          .HasOne(p => p.Ativo)
          .WithMany()
          .HasForeignKey(p => p.AtivoId);
    }
}
