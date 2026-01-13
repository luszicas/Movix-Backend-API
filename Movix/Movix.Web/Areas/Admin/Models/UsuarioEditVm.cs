using System.ComponentModel.DataAnnotations;

namespace Movix.Web.Areas.Admin.Models;

public class UsuarioListFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Q { get; set; }            // email ou username
    public string? Role { get; set; }         // role exata
    public bool? Ativo { get; set; }          // true/false/null
}

public record UsuarioRowVm(
    string Id,
    string Email,
    string UserName,
    bool IsActive,
    string[] Roles
);

public record PagedUsuario(int Page, int PageSize, int Total, IReadOnlyList<UsuarioRowVm> Items);

public class UsuarioCreateVm
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(256)]
    public string UserName { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = ""; // será validada contra RoleManager
}

public class UsuarioEditVm
{
    [Required]
    public string Id { get; set; } = "";

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(256)]
    public string UserName { get; set; } = "";

    [Required]
    public string Role { get; set; } = ""; // uma role ativa
}
