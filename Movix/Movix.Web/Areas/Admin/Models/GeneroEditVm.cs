using System.ComponentModel.DataAnnotations;

namespace Movix.Web.Areas.Admin.Models;

public class GeneroEditVm
{
    public int? Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = "";
}

public class GeneroListFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Q { get; set; }
}

public record GeneroRowVm(int Id, string Nome);
public record PagedGenero(int Page, int PageSize, int Total, IReadOnlyList<GeneroRowVm> Items);
