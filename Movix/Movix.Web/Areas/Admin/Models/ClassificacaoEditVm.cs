using System.ComponentModel.DataAnnotations;

namespace Movix.Web.Areas.Admin.Models;

public class ClassificacaoEditVm
{
    public int? Id { get; set; }

    [Required, StringLength(50)]
    public string Nome { get; set; } = "";
}

public class ClassificacaoListFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Q { get; set; }
}

public record ClassificacaoRowVm(int Id, string Nome);
public record PagedClassificacao(int Page, int PageSize, int Total, IReadOnlyList<ClassificacaoRowVm> Items);
