using Movix.Domain.Entities;

namespace Movix.Application.Abstractions.Repositories;

public interface IClassificacaoRepository
{
    Task<IReadOnlyList<Classificacao>> GetAllAsync(CancellationToken ct = default);
}
