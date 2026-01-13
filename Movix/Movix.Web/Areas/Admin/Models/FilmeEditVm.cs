using System.ComponentModel.DataAnnotations;

namespace Movix.Web.Areas.Admin.Models;

public class FilmeEditVm
{
    public int? Id { get; set; }

    [Required, StringLength(200)]
    public string Titulo { get; set; } = "";

    [StringLength(2000)]
    public string? Sinopse { get; set; }

    [Range(1900, 2100)]
    public int Ano { get; set; } = DateTime.UtcNow.Year;

    // Caminho/URL da capa (preenchido por upload OU URL manual)
    [Display(Name = "URL da capa")]
    [Url(ErrorMessage = "URL inválida.")]
    public string? ImagemCapaUrl { get; set; }

    // 🆕 Novo campo: URL do trailer
    [Display(Name = "URL do trailer")]
    [Url(ErrorMessage = "URL do trailer inválida.")]
    public string? UrlTrailer { get; set; }

    [Required]
    [Display(Name = "Gênero")]
    public int GeneroId { get; set; }

    [Required]
    [Display(Name = "Classificação")]
    public int ClassificacaoId { get; set; }
}

public class FilmeListFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Q { get; set; }
    public int? GeneroId { get; set; }
    public int? ClassificacaoId { get; set; }
    public int? Ano { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool Desc { get; set; } = true;
}

public record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);
public record FilmeRowVm(int Id, string Titulo, int Ano, string Genero, string Classificacao);
