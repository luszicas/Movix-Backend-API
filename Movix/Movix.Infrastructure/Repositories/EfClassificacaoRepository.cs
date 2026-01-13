using Microsoft.EntityFrameworkCore;
using Movix.Application.Abstractions.Repositories;
using Movix.Domain.Entities;
using Movix.Infrastructure.Persistence;

namespace Movix.Infrastructure.Repositories;

public sealed class EfClassificacaoRepository : IClassificacaoRepository
{
    private readonly AppDbContext _ctx;
    public EfClassificacaoRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Classificacao>> GetAllAsync(CancellationToken ct = default) =>
        await _ctx.Classificacoes.AsNoTracking().OrderBy(c => c.Nome).ToListAsync(ct);
}
