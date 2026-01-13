using Movix.Web.Models;

namespace Movix.Web.Services;

public interface IApiClient
{
    Task<IReadOnlyList<(int Id, string Nome)>> GetGenerosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<(int Id, string Nome)>> GetClassificacoesAsync(CancellationToken ct = default);
    Task<PagedResult<FilmeDto>> SearchFilmesAsync(CatalogFilterVm filter, CancellationToken ct = default);
    Task<FilmeDto?> GetFilmeAsync(int id, CancellationToken ct = default);
}
