using Movix.Application.Abstractions.Repositories;
using Movix.Application.DTOs;
using Movix.Application.Filters;
using Movix.Application.Services;
                 // Contrato IFilmeQueryService implementado por esta classe.

namespace Movix.Infrastructure.Services;           // Camada Infra: implementação concreta de serviços de aplicação.

/// <summary>
/// Serviço de consulta que conecta o repositório (Infra) às projeções em DTO (Application).
/// Orquestra filtros/paginação e projeta entidades de domínio em <see cref="FilmeDto"/>.
/// </summary>
public sealed class FilmeQueryService : IFilmeQueryService // 'sealed' evita herança acidental; favorece previsibilidade.
{
    private readonly IFilmeRepository _repo;               // Dependência: repositório de leitura de filmes.

    public FilmeQueryService(IFilmeRepository repo) => _repo = repo; // DI por construtor (DIP). Testável com mocks/fakes.

    /// <summary>
    /// Executa busca paginada de filmes usando <see cref="FilmeFilter"/> e retorna <see cref="PagedResult{T}"/> de <see cref="FilmeDto"/>.
    /// Suporta filter.PageSize == 0 para retornar todos os itens (total) em uma só resposta.
    /// </summary>
    public async Task<PagedResult<FilmeDto>> SearchAsync(FilmeFilter filter, CancellationToken ct = default)
    {
        var (items, total) = await _repo.SearchAsync(filter, ct);

        var dtos = items.Select(f => new FilmeDto(
            f.Id,
            f.Titulo,
            f.Sinopse,
            f.Ano,
            f.ImagemCapaUrl,
            f.GeneroId,
            f.Genero?.Nome ?? "",
            f.ClassificacaoId,
            f.Classificacao?.Nome ?? "",
            f.UrlTrailer
        )).ToList();

        var page = filter.Page < 1 ? 1 : filter.Page;

        // Se pageSize == 0 => cliente pediu todos os itens; definimos pageSize = total para o metadado.
        int pageSize;
        if (filter.PageSize == 0)
        {
            pageSize = total;
        }
        else
        {
            pageSize = filter.PageSize < 1 ? 12 : filter.PageSize;
            if (pageSize > 2000) pageSize = 2000; // mesma proteção do repo
        }

        return new PagedResult<FilmeDto>(page, pageSize, total, dtos);
    }
}