using Movix.Application.DTOs;
using Movix.Application.Filters;

namespace Movix.Application.Services; // Camada Application: serviços orquestram casos de uso sem dependências de Infra concretas.

/// <summary>
/// Serviço de aplicação para consultar o catálogo com filtros/paginação.
/// Expõe consultas em termos de DTOs (e não entidades de domínio) para evitar vazar regras internas.
/// </summary>
public interface IFilmeQueryService
{
    /// <summary>
    /// Pesquisa filmes aplicando filtros, ordenação e paginação e retorna um resultado paginado.
    /// </summary>
    /// <param name="filter">Critérios de filtro/paginação/ordenação (ex.: Gênero, Classificação, Ano, texto, SortBy, Desc, Page, PageSize).</param>
    /// <param name="ct">Token de cancelamento para cooperação em operações I/O bound.</param>
    /// <returns>
    /// Um <see cref="PagedResult{T}"/> de <see cref="FilmeDto"/>, contendo itens da página corrente e metadados (ex.: Total).
    /// </returns>
    Task<PagedResult<FilmeDto>> SearchAsync(FilmeFilter filter, CancellationToken ct = default); // Retorna DTOs para consumo direto na UI/API.
}
