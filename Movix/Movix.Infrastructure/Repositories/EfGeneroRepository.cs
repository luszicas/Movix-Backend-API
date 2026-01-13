using Microsoft.EntityFrameworkCore;
using Movix.Application.Abstractions.Repositories;
using Movix.Domain.Entities;
using Movix.Infrastructure.Persistence;

namespace Movix.Infrastructure.Repositories;

public sealed class EfGeneroRepository : IGeneroRepository
{
    private readonly AppDbContext _ctx;
    public EfGeneroRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Genero>> GetAllAsync(CancellationToken ct = default) =>
        await _ctx.Generos.AsNoTracking().OrderBy(g => g.Nome).ToListAsync(ct);
}
