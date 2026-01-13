using Movix.Application.DTOs;

namespace Movix.Application.Filters;

//public sealed class FilmeFilter : PagedRequest
//{
//    public int? GeneroId { get; set; }
//    public int? ClassificacaoId { get; set; }
//    public int? Ano { get; set; }
//    public string? Q { get; set; }
//    public string? SortBy { get; set; }
//    public bool Desc { get; set; } = true;
//}

public sealed class FilmeFilter : PagedRequest
{
    public int? GeneroId { get; set; }
    public int? ClassificacaoId { get; set; }
    public int? Ano { get; set; }
    public string? Q { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; } = true;
}
