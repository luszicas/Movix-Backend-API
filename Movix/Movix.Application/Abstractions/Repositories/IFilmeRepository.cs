using Movix.Application.Filters;
using Movix.Domain.Entities;

namespace Movix.Application.Abstractions.Repositories;

public interface IFilmeRepository
{
    Task<(IReadOnlyList<Filme> Items, int Total)> SearchAsync(FilmeFilter filter, CancellationToken ct = default);
    Task<Filme?> GetByIdAsync(int id, CancellationToken ct = default);
}
