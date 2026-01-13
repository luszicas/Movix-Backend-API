namespace Movix.Web.Models;

public class CatalogFilterVm
{
    public int? GeneroId { get; set; }
    public int? ClassificacaoId { get; set; }
    public int? Ano { get; set; }
    public string? Q { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool Desc { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class CatalogPageVm
{
    public CatalogFilterVm Filter { get; set; } = new();
    public PagedResult<FilmeDto>? PageResult { get; set; }
    public IReadOnlyList<(int Id, string Nome)> Generos { get; set; } = Array.Empty<(int, string)>();
    public IReadOnlyList<(int Id, string Nome)> Classificacoes { get; set; } = Array.Empty<(int, string)>();
}
