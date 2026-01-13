using Movix.Domain.Entities;

namespace Movix.Application.Abstractions.Repositories;

public interface IGeneroRepository
{
    Task<IReadOnlyList<Genero>> GetAllAsync(CancellationToken ct = default);
}
