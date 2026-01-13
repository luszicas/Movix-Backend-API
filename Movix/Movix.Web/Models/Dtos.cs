namespace Movix.Web.Models; // Namespace da camada Web (modelos usados pelas Views/Controllers).

// espelha a resposta da API
public record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);
// 'record' adequado para DTOs imutáveis por valor e com equality estrutural.
// Page: página atual; PageSize: tamanho efetivo da página; Total: total de itens filtrados; Items: coleção somente leitura.

// DTO projetado para a Web consumindo a API (espelha o contrato do backend).
public record FilmeDto(
    int Id,                       // Identificador único do filme.
    string Titulo,                // Título para exibição.
    string? Sinopse,              // Sinopse opcional (pode ser null).
    int Ano,                      // Ano de lançamento (YYYY).
    string? ImagemCapaUrl,        // URL da capa (pode ser null/empty).
    int GeneroId,                 // FK do gênero (útil para binds/edição).
    string GeneroNome,            // Nome do gênero (exibição).
    int ClassificacaoId,          // FK da classificação indicativa.
    string ClassificacaoNome,
    string? UrlTrailer);    // Nome da classificação (exibição).
// Observação: manter nomes e tipos alinhados com o contrato da API para desserialização direta (System.Text.Json).
