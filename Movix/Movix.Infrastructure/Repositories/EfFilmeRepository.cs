using Microsoft.EntityFrameworkCore;                         // EF Core: IQueryable, Include, AsNoTracking, CountAsync, ToListAsync, etc.

using Movix.Application.Abstractions.Repositories;   // Contrato IFilmeRepository (camada Application).
using Movix.Application.Filters;                     // Record FilmeFilter (filtros + paginação/ordenação).
using Movix.Domain.Entities;                         // Entidade de domínio Filme.
using Movix.Infrastructure.Persistence;               // AppDbContext (DbContext do EF Core).

namespace Movix.Infrastructure.Repositories;          // Camada Infra: implementação concreta de repositório.

/// <summary>
/// Implementação EF Core do repositório de filmes.
/// </summary>
public sealed class EfFilmeRepository : IFilmeRepository      // 'sealed' evita herança acidental e ajuda o JIT.
{
    private readonly AppDbContext _ctx;                       // DbContext injetado (lifetime típico: Scoped em ASP.NET Core).

    public EfFilmeRepository(AppDbContext ctx) => _ctx = ctx; // DI por construtor: facilita testes e substituição por mocks.

    public async Task<(IReadOnlyList<Filme> Items, int Total)> SearchAsync(
        FilmeFilter filter, CancellationToken ct = default)
    {
        // Saneamento de paginação (agora suportamos pageSize == 0 => retorna todos os itens)
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize; // mantemos o valor do cliente; 0 = sem paginação

        // Limites razoáveis para evitar payloads absurdos (mas 0 significa "tudo")
        if (pageSize > 2000) pageSize = 2000; // se cliente pedir muito, limitamos para 2000 por segurança

        var q = _ctx.Filmes
            .AsNoTracking()                                   // Leitura somente; desativa ChangeTracker → melhor performance.
            .Include(f => f.Genero)                           // Carrega navegação 'Genero' (JOIN) para evitar N+1.
            .Include(f => f.Classificacao)                    // Carrega navegação 'Classificacao'.
            .AsQueryable();                                   // Mantém composição de query (adiar execução).

        // Filtros
        if (filter.GeneroId is int gid)
            q = q.Where(f => f.GeneroId == gid);

        if (filter.ClassificacaoId is int cid)
            q = q.Where(f => f.ClassificacaoId == cid);

        if (filter.Ano is int ano)
            q = q.Where(f => f.Ano == ano);

        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var like = $"%{filter.Q.Trim()}%";
            q = q.Where(f =>
                EF.Functions.Like(f.Titulo, like) ||
                (f.Sinopse != null && EF.Functions.Like(f.Sinopse, like)));
        }

        // Ordenação
        q = (filter.SortBy?.ToLowerInvariant()) switch
        {
            "titulo" => filter.Desc ? q.OrderByDescending(f => f.Titulo) : q.OrderBy(f => f.Titulo),
            "ano" => filter.Desc ? q.OrderByDescending(f => f.Ano) : q.OrderBy(f => f.Ano),
            "createdat" => filter.Desc ? q.OrderByDescending(f => f.CreatedAt) : q.OrderBy(f => f.CreatedAt),
            _ => q.OrderByDescending(f => f.CreatedAt)
        };

        var total = await q.CountAsync(ct); // total após filtros

        // Se pageSize == 0 -> traz todos os itens (útil para views que querem tudo)
        if (filter.PageSize == 0)
        {
            var allItems = await q.ToListAsync(ct);
            return (allItems, total);
        }

        // Paginação normal quando pageSize > 0
        var effectivePageSize = pageSize < 1 ? 12 : pageSize;
        var items = await q
            .Skip((page - 1) * effectivePageSize)
            .Take(effectivePageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Filme?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _ctx.Filmes
            .AsNoTracking()
            .Include(f => f.Genero)
            .Include(f => f.Classificacao)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
}