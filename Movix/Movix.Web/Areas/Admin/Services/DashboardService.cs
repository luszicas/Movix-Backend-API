using Microsoft.EntityFrameworkCore;

using Movix.Infrastructure.Persistence;

namespace Movix.Web.Areas.Admin.Services;

public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    // ---------- Cards ----------
    public async Task<int> TotalFilmesAsync(CancellationToken ct = default)
        => await _db.Filmes.AsNoTracking().CountAsync(ct);

    public async Task<int> TotalGenerosAsync(CancellationToken ct = default)
        => await _db.Generos.AsNoTracking().CountAsync(ct);

    public async Task<int> TotalUsuariosAsync(CancellationToken ct = default)
        => await _db.Users.AsNoTracking().CountAsync(ct);

    public async Task<Dictionary<string, int>> TotalUsuariosPorRoleAsync(CancellationToken ct = default)
    {
        // Junta UserRoles -> Roles para obter nomes de roles
        var query =
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            group ur by r.Name! into g
            select new { Role = g.Key, Qtd = g.Count() };

        return await query.AsNoTracking().ToDictionaryAsync(x => x.Role, x => x.Qtd, ct);
    }

    // ---------- Gráficos ----------
    public async Task<List<(string Label, int Qtd)>> FilmesPorGeneroAsync(CancellationToken ct = default)
    {
        var query =
            from f in _db.Filmes
            join g in _db.Generos on f.GeneroId equals g.Id
            group f by g.Nome into grp
            orderby grp.Key
            select new { Label = grp.Key, Qtd = grp.Count() };

        var data = await query.AsNoTracking().ToListAsync(ct);
        return data.Select(x => (x.Label, x.Qtd)).ToList();
    }

    public async Task<List<(string Label, int Qtd)>> FilmesPorClassificacaoAsync(CancellationToken ct = default)
    {
        var query =
            from f in _db.Filmes
            join c in _db.Classificacoes on f.ClassificacaoId equals c.Id
            group f by c.Nome into grp
            orderby grp.Key
            select new { Label = grp.Key, Qtd = grp.Count() };

        var data = await query.AsNoTracking().ToListAsync(ct);
        return data.Select(x => (x.Label, x.Qtd)).ToList();
    }

    public async Task<List<(int Ano, int Qtd)>> FilmesPorAnoAsync(CancellationToken ct = default)
    {
        var data = await _db.Filmes
            .AsNoTracking()
            .GroupBy(f => f.Ano)
            .Select(g => new { Ano = g.Key, Qtd = g.Count() })
            .OrderBy(x => x.Ano)
            .ToListAsync(ct);

        return data.Select(x => (x.Ano, x.Qtd)).ToList();
    }

    // ---------- Tabela auxiliar ----------
    public async Task<List<UltimoFilmeItem>> UltimosFilmesAsync(int take = 5, CancellationToken ct = default)
    {
        var q = from f in _db.Filmes
                join g in _db.Generos on f.GeneroId equals g.Id
                join c in _db.Classificacoes on f.ClassificacaoId equals c.Id
                orderby f.CreatedAt descending
                select new UltimoFilmeItem
                {
                    Id = f.Id,
                    Titulo = f.Titulo,
                    Ano = f.Ano,
                    Genero = g.Nome,
                    Classificacao = c.Nome,
                    CreatedAt = f.CreatedAt
                };

        return await q.AsNoTracking().Take(take).ToListAsync(ct);
    }
}

public class UltimoFilmeItem
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public int Ano { get; set; }
    public string Genero { get; set; } = "";
    public string Classificacao { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
