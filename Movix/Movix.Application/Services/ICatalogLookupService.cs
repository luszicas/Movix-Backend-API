//namespace Movix.Application.Services;

//public interface ICatalogLookupService
//{
//    Task<IReadOnlyList<(int Id, string Nome)>> GetGenerosAsync(CancellationToken ct = default);
//    Task<IReadOnlyList<(int Id, string Nome)>> GetClassificacoesAsync(CancellationToken ct = default);
//}


using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Movix.Application.Services
{
    /// <summary>
    /// Fornece listas auxiliares para dropdowns e filtros da aplicação.
    /// </summary>
    public interface ICatalogLookupService
    {
        /// <summary>
        /// Retorna a lista de gêneros (Id, Nome).
        /// </summary>
        Task<IReadOnlyList<object>> GetGenerosAsync(CancellationToken ct = default);

        /// <summary>
        /// Retorna a lista de classificações (Id, Nome).
        /// </summary>
        Task<IReadOnlyList<object>> GetClassificacoesAsync(CancellationToken ct = default);
    }
}
