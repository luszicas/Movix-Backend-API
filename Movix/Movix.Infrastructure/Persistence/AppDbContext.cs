using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Movix.Domain.Entities;
using Movix.Infrastructure.Entities;

namespace Movix.Infrastructure.Persistence;

public class AppDbContext :
    IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Genero> Generos => Set<Genero>();
    public DbSet<Classificacao> Classificacoes => Set<Classificacao>();
    public DbSet<Filme> Filmes => Set<Filme>();
    public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Tabelas
        b.Entity<Genero>().ToTable("Generos");
        b.Entity<Classificacao>().ToTable("Classificacoes");
        b.Entity<Filme>().ToTable("Filmes");
        b.Entity<TipoUsuario>().ToTable("TiposUsuario");

        // Genero
        b.Entity<Genero>()
            .Property(p => p.Nome).HasMaxLength(80).IsRequired();

        // Classificacao
        b.Entity<Classificacao>()
            .Property(p => p.Nome).HasMaxLength(10).IsRequired();

        // Filme
        b.Entity<Filme>()
            .Property(p => p.Titulo).HasMaxLength(180).IsRequired();

        b.Entity<Filme>()
            .HasOne(f => f.Genero)
            .WithMany(g => g.Filmes)
            .HasForeignKey(f => f.GeneroId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Filme>()
            .HasOne(f => f.Classificacao)
            .WithMany(c => c.Filmes)
            .HasForeignKey(f => f.ClassificacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Filme>().HasIndex(f => f.Titulo);
        b.Entity<Filme>().HasIndex(f => f.Ano);
        b.Entity<Filme>().HasIndex(f => new { f.GeneroId, f.ClassificacaoId });

        // ApplicationUser
        b.Entity<ApplicationUser>().ToTable("AspNetUsers"); // mantém convenção Identity
        b.Entity<ApplicationUser>().HasIndex(u => u.Email).IsUnique();

        // Filtro global para usuários inativos (opcional no contexto)
        b.Entity<ApplicationUser>().HasQueryFilter(u => u.IsActive);
    }
}
