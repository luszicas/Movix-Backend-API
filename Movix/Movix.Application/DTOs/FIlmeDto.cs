namespace Movix.Application.DTOs;

/// <summary>
/// DTO enxuto para listagem de filmes no catálogo.
/// </summary>
public record FilmeDto(
    int Id,
    string Titulo,
    string? Sinopse,
    int Ano,
    string? ImagemCapaUrl,
    int GeneroId,
    string GeneroNome,
    int ClassificacaoId,
    string ClassificacaoNome,
    string? UrlTrailer);




