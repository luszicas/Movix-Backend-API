//using Movix.Application.Abstractions.Repositories;
//using Movix.Application.Services;

//namespace Movix.Infrastructure.Services;

///// <summary>
///// Fornece listas auxiliares (gêneros e classificações) em forma simples.
///// Implementa projeções leves (Id, Nome) para consumo direto pela UI/API.
///// </summary>
//public sealed class CatalogLookupService : ICatalogLookupService
//{
//    private readonly IGeneroRepository _generos;
//    private readonly IClassificacaoRepository _classificacoes;

//    // Injeção de dependências via construtor (Scoped DI típico)
//    public CatalogLookupService(IGeneroRepository generos, IClassificacaoRepository classificacoes)
//    {
//        _generos = generos;
//        _classificacoes = classificacoes;
//    }

//    /// <summary>
//    /// Retorna lista de gêneros como tuplas (Id, Nome).
//    /// </summary>
//    /// <param name="ct">Token de cancelamento opcional.</param>
//    /// <returns>Coleção somente leitura de tuplas (Id, Nome).</returns>
//    public async Task<IReadOnlyList<(int Id, string Nome)>> GetGenerosAsync(CancellationToken ct = default)
//    {
//        var list = await _generos.GetAllAsync(ct);          // Busca entidades sem tracking
//        return list.Select(g => (g.Id, g.Nome)).ToList();   // Projeta para tupla
//    }

//    /// <summary>
//    /// Retorna lista de classificações como tuplas (Id, Nome).
//    /// </summary>
//    /// <param name="ct">Token de cancelamento opcional.</param>
//    /// <returns>Coleção somente leitura de tuplas (Id, Nome).</returns>
//    public async Task<IReadOnlyList<(int Id, string Nome)>> GetClassificacoesAsync(CancellationToken ct = default)
//    {
//        var list = await _classificacoes.GetAllAsync(ct);   // Busca entidades sem tracking
//        return list.Select(c => (c.Id, c.Nome)).ToList();   // Projeta para tupla
//    }
//}


using Movix.Application.Abstractions.Repositories;
using Movix.Application.Services;

namespace Movix.Infrastructure.Services;

/// <summary>
/// Fornece listas auxiliares (gêneros e classificações) em forma simples.
/// Implementa projeções leves (Id, Nome) para consumo direto pela UI/API.
/// </summary>
public sealed class CatalogLookupService : ICatalogLookupService
{
    private readonly IGeneroRepository _generos;
    private readonly IClassificacaoRepository _classificacoes;

    // Injeção de dependências via construtor (Scoped DI típico)
    public CatalogLookupService(IGeneroRepository generos, IClassificacaoRepository classificacoes)
    {
        _generos = generos;
        _classificacoes = classificacoes;
    }

    /// <summary>
    /// Retorna lista de gêneros (Id, Nome).
    /// </summary>
    public async Task<IReadOnlyList<object>> GetGenerosAsync(CancellationToken ct = default)
    {
        var list = await _generos.GetAllAsync(ct);
        return list
            .Select(g => new { g.Id, g.Nome })
            .ToList<object>();
    }

    /// <summary>
    /// Retorna lista de classificações (Id, Nome).
    /// </summary>
    public async Task<IReadOnlyList<object>> GetClassificacoesAsync(CancellationToken ct = default)
    {
        var list = await _classificacoes.GetAllAsync(ct);
        return list
            .Select(c => new { c.Id, c.Nome })
            .ToList<object>();
    }
}


