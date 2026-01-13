

using Microsoft.AspNetCore.Mvc;                                  // MVC: atributos de roteamento, ControllerBase, ActionResult, etc.
using Movix.Application.DTOs;                            // DTOs expostos pela camada Application (FilmeDto, PagedResult<T>).
using Movix.Application.Filters;                         // Filtro de consulta/paginação (FilmeFilter).
using Movix.Application.Services;                        // Serviço de aplicação para consultas (IFilmeQueryService).
using Movix.Application.Abstractions.Repositories;       // Abstração de repositório (IFilmeRepository) para busca por Id.

namespace Movix.Api.Controllers;                         // Camada Web/API.

[ApiController]                                                  // Habilita model binding/validation automáticos e ProblemDetails (400).
[Route("api/[controller]")]                                      // Base route: "api/filmes" (nome do controller sem "Controller").
public class FilmesController : ControllerBase
{
    private readonly IFilmeQueryService _service;               // Serviço de consulta paginada (retorna DTOs).
    private readonly IFilmeRepository _repo;                    // Repositório de leitura para obter um filme por Id.

    public FilmesController(IFilmeQueryService service, IFilmeRepository repo)
    {
        _service = service;                                     // Injeta a orquestração de consultas (Application).
        _repo = repo;                                           // Injeta repositório (Infra via abstração) para GetById.
    }

    /// <summary>Consulta paginada com filtros/ordenação.</summary>
    [HttpGet]                                                   // GET api/filmes
    public async Task<ActionResult<PagedResult<FilmeDto>>> Get([FromQuery] FilmeFilter filter, CancellationToken ct)
    {
        var result = await _service.SearchAsync(filter, ct);    // Delega a consulta ao serviço (DTOs + metadados paginados).
        Response.Headers["X-Total-Count"] = result.Total.ToString(); // Expõe total no header (útil para grids/clientes).
        return Ok(result);                                      // 200 OK com PagedResult<FilmeDto> no corpo.
    }

    /// <summary>Detalhe por Id.</summary>
    [HttpGet("{id:int}")]                                       // GET api/filmes/{id}
    public async Task<ActionResult<FilmeDto>> GetById(int id, CancellationToken ct)
    {
        var f = await _repo.GetByIdAsync(id, ct);               // Busca entidade pelo repositório (inclui navegações).
        if (f is null) return NotFound();                       // 404 quando não encontrado.

        var dto = new FilmeDto(                                 // Projeta entidade em DTO (não vaza domínio para a UI).
            f.Id,                                               // Id do filme.
            f.Titulo,                                           // Título.
            f.Sinopse,                                          // Sinopse (pode ser null).
            f.Ano,                                              // Ano de lançamento.
            f.ImagemCapaUrl,                                    // URL da capa (pode ser null).
            f.GeneroId,                                         // FK do gênero.
            f.Genero.Nome,                                      // Nome do gênero (navegação carregada pelo repo).
            f.ClassificacaoId,                                  // FK da classificação.
            f.Classificacao.Nome,
            f.UrlTrailer// Nome da classificação (navegação carregada pelo repo).
        );

        return Ok(dto);                                         // 200 OK com o DTO do filme.
    }
}
