using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movix.Infrastructure.Entities;
using Movix.Infrastructure.Persistence;
using Movix.Web.Areas.Admin.Models;

namespace Movix.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ManagerOrAdmin")]
public class UsuariosController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole<Guid>> _roles;

    public UsuariosController(
        AppDbContext db,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<Guid>> roles)
    {
        _db = db;
        _users = users;
        _roles = roles;
    }

    // LISTA
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] UsuarioListFilter f)
    {
        f.Page = Math.Max(1, f.Page);
        f.PageSize = Math.Clamp(f.PageSize, 5, 50);

        var query = _db.Users.IgnoreQueryFilters().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var term = f.Q.Trim();
            query = query.Where(u => (u.Email ?? "").Contains(term) || (u.UserName ?? "").Contains(term));
        }
        if (f.Ativo is bool ativo)
            query = query.Where(u => u.IsActive == ativo);

        var total = await query.CountAsync();

        var page = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .Select(u => new { u.Id, u.Email, u.UserName, u.IsActive })
            .ToListAsync();

        var userIds = page.Select(p => p.Id).ToArray();

        var rolesMap = await _db.Roles
            .Select(r => new { r.Id, r.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name!);

        var userRolesLookup = await _db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .GroupBy(ur => ur.UserId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(ur => rolesMap.TryGetValue(ur.RoleId, out var n) ? n : "?").ToArray()
            );

        var items = page.Select(p =>
            new UsuarioRowVm(
                p.Id.ToString(),
                p.Email ?? "",
                p.UserName ?? "",
                p.IsActive,
                userRolesLookup.TryGetValue(p.Id, out var arr) ? arr : Array.Empty<string>())
        ).ToList();

        ViewBag.Roles = await _roles.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync();
        ViewBag.Filter = f;

        return View("~/Areas/Admin/Views/Usuarios/Index.cshtml",
            new PagedUsuario(f.Page, f.PageSize, total, items));
    }

    // CREATE (GET/POST)
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CarregarRolesAsync();
        return View("~/Areas/Admin/Views/Usuarios/Create.cshtml", new UsuarioCreateVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCreateVm vm)
    {
        await CarregarRolesAsync();

        if (!await RoleExisteAsync(vm.Role))
            ModelState.AddModelError(nameof(vm.Role), "Role inexistente.");

        if (!ModelState.IsValid)
            return View("~/Areas/Admin/Views/Usuarios/Create.cshtml", vm);

        if (IsAdminRole(vm.Role) && !UserIsAdmin())
            return Forbid();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = vm.Email.Trim(),
            NormalizedEmail = vm.Email.Trim().ToUpperInvariant(),
            UserName = vm.UserName.Trim(),
            NormalizedUserName = vm.UserName.Trim().ToUpperInvariant(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var res = await _users.CreateAsync(user, vm.Password);
        if (!res.Succeeded)
        {
            foreach (var e in res.Errors) ModelState.AddModelError("", e.Description);
            return View("~/Areas/Admin/Views/Usuarios/Create.cshtml", vm);
        }

        var addRoleRes = await _users.AddToRoleAsync(user, await RoleRealNameAsync(vm.Role));
        if (!addRoleRes.Succeeded)
        {
            foreach (var e in addRoleRes.Errors) ModelState.AddModelError("", e.Description);
            return View("~/Areas/Admin/Views/Usuarios/Create.cshtml", vm);
        }

        TempData["ok"] = "Usuário criado.";
        return RedirectToAction(nameof(Index));
    }

    // EDIT (GET/POST)
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (!Guid.TryParse(id, out var gid)) return NotFound();

        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == gid);
        if (user == null) return NotFound();

        await CarregarRolesAsync();

        var roles = await (from ur in _db.UserRoles
                           join r in _db.Roles on ur.RoleId equals r.Id
                           where ur.UserId == gid
                           select r.Name!).ToListAsync();

        var vm = new UsuarioEditVm
        {
            Id = id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            Role = roles.FirstOrDefault() ?? ""
        };

        return View("~/Areas/Admin/Views/Usuarios/Edit.cshtml", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UsuarioEditVm vm)
    {
        if (id != vm.Id || !Guid.TryParse(id, out var gid)) return BadRequest();

        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == gid);
        if (user == null) return NotFound();

        await CarregarRolesAsync();

        if (!await RoleExisteAsync(vm.Role))
            ModelState.AddModelError(nameof(vm.Role), "Role inexistente.");

        if (!ModelState.IsValid)
            return View("~/Areas/Admin/Views/Usuarios/Edit.cshtml", vm);

        if (IsAdminRole(vm.Role) && !UserIsAdmin())
            return Forbid();

        user.Email = vm.Email.Trim();
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.UserName = vm.UserName.Trim();
        user.NormalizedUserName = user.UserName.ToUpperInvariant();

        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        var current = await _db.UserRoles.Where(ur => ur.UserId == gid).ToListAsync();
        if (current.Any())
        {
            _db.UserRoles.RemoveRange(current);
            await _db.SaveChangesAsync();
        }

        var roleName = await RoleRealNameAsync(vm.Role);
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role != null)
        {
            _db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = gid, RoleId = role.Id });
            await _db.SaveChangesAsync();
        }

        TempData["ok"] = "Usuário atualizado.";
        return RedirectToAction(nameof(Index));
    }

    // ====== RESTRITO AO ADMIN ======

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Desativar(string id)
    {
        if (!UserIsAdmin()) return Forbid(); // defesa extra
        if (!Guid.TryParse(id, out var gid)) return BadRequest();

        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == gid);
        if (user == null) return NotFound();

        if (User.Identity?.Name?.Equals(user.UserName, StringComparison.OrdinalIgnoreCase) == true)
            return BadRequest("Você não pode desativar sua própria conta.");

        user.IsActive = false;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Usuário desativado (soft delete).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Reativar(string id)
    {
        if (!UserIsAdmin()) return Forbid(); // defesa extra
        if (!Guid.TryParse(id, out var gid)) return BadRequest();

        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == gid);
        if (user == null) return NotFound();

        user.IsActive = true;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Usuário reativado.";
        return RedirectToAction(nameof(Index));
    }

    // helpers ----------------------------------------------------
    private async Task CarregarRolesAsync()
        => ViewBag.AllRoles = await _roles.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync();

    private bool UserIsAdmin()
        => User.IsInRole("Admin") || User.IsInRole("ADMIN") || User.IsInRole("Administrador");

    private async Task<bool> RoleExisteAsync(string roleInput)
    {
        var real = await RoleRealNameAsync(roleInput);
        return !string.IsNullOrWhiteSpace(real) && await _roles.RoleExistsAsync(real);
    }

    private async Task<string> RoleRealNameAsync(string roleInput)
    {
        if (string.IsNullOrWhiteSpace(roleInput)) return "";
        var all = await _roles.Roles.Select(r => r.Name!).ToListAsync();

        string? find(IEnumerable<string> aliases)
            => all.FirstOrDefault(n => aliases.Contains(n, StringComparer.OrdinalIgnoreCase));

        var admin = find(new[] { "Admin", "ADMIN", "Administrador" });
        var ger = find(new[] { "Gerente", "GERENTE" });
        var outros = find(new[] { "Outros", "OUTROS" });

        if (all.Contains(roleInput, StringComparer.OrdinalIgnoreCase)) return roleInput;
        if (admin is not null && roleInput.Equals(admin, StringComparison.OrdinalIgnoreCase)) return admin;
        if (ger is not null && roleInput.Equals(ger, StringComparison.OrdinalIgnoreCase)) return ger;
        if (outros is not null && roleInput.Equals(outros, StringComparison.OrdinalIgnoreCase)) return outros;

        return all.FirstOrDefault(n => n.Equals(roleInput, StringComparison.OrdinalIgnoreCase)) ?? "";
    }

    private static bool IsAdminRole(string roleInput)
        => new[] { "Admin", "ADMIN", "Administrador" }.Any(a => roleInput.Equals(a, StringComparison.OrdinalIgnoreCase));
}
