using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movix.Infrastructure.Persistence;
using Movix.Web.Areas.Admin.Models;

namespace Movix.Web.Areas.Admin.Controllers;


using Microsoft.Extensions.Caching.Memory;
using Movix.Infrastructure.Persistence;
using Movix.Web.Areas.Admin.Models;

using Movix.Web.Shared; // CacheKeys


[Area("Admin")]
[Authorize(Policy = "ManagerOrAdmin")]
public class GenerosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public GenerosController(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] GeneroListFilter f, CancellationToken ct)
    {
        f.Page = Math.Max(1, f.Page);
        f.PageSize = Math.Clamp(f.PageSize, 5, 50);

        var q = _db.Generos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var term = f.Q.Trim();
            q = q.Where(x => x.Nome.Contains(term));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.Nome)
                           .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize)
                           .Select(x => new GeneroRowVm(x.Id, x.Nome))
                           .ToListAsync(ct);

        var model = new PagedGenero(f.Page, f.PageSize, total, items);
        ViewBag.Filter = f;
        return View("~/Areas/Admin/Views/Generos/Index.cshtml", model);
    }

    [HttpGet]
    public IActionResult Create() =>
        View("~/Areas/Admin/Views/Generos/Create.cshtml", new GeneroEditVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GeneroEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("~/Areas/Admin/Views/Generos/Create.cshtml", vm);

        var exists = await _db.Generos.AnyAsync(g => g.Nome == vm.Nome.Trim(), ct);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Nome), "Já existe um gênero com esse nome.");
            return View("~/Areas/Admin/Views/Generos/Create.cshtml", vm);
        }

        _db.Generos.Add(new Movix.Domain.Entities.Genero { Nome = vm.Nome.Trim() });
        await _db.SaveChangesAsync(ct);

        _cache.Remove(CacheKeys.Generos);


        TempData["ok"] = "Gênero criado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var g = await _db.Generos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g == null) return NotFound();

        var vm = new GeneroEditVm { Id = g.Id, Nome = g.Nome };
        return View("~/Areas/Admin/Views/Generos/Edit.cshtml", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GeneroEditVm vm, CancellationToken ct)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View("~/Areas/Admin/Views/Generos/Edit.cshtml", vm);

        var otherExists = await _db.Generos.AnyAsync(g => g.Id != id && g.Nome == vm.Nome.Trim(), ct);
        if (otherExists)
        {
            ModelState.AddModelError(nameof(vm.Nome), "Já existe outro gênero com esse nome.");
            return View("~/Areas/Admin/Views/Generos/Edit.cshtml", vm);
        }

        var g = await _db.Generos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g == null) return NotFound();

        g.Nome = vm.Nome.Trim();
        await _db.SaveChangesAsync(ct);

        _cache.Remove(CacheKeys.Generos);


        TempData["ok"] = "Gênero atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var g = await _db.Generos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g == null) return NotFound();
        return View("~/Areas/Admin/Views/Generos/Delete.cshtml", g);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("ADMIN") || User.IsInRole("Administrador");
        if (!isAdmin) return Forbid();

        var g = await _db.Generos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g == null) return NotFound();

        // 1) Bloqueio explícito e mensagem
        var hasFilms = await _db.Filmes.AnyAsync(f => f.GeneroId == id, ct);
        if (hasFilms)
        {
            TempData["error"] = "Não é possível excluir: existem filmes vinculados a este gênero.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _db.Generos.Remove(g);
            await _db.SaveChangesAsync(ct);

            _cache.Remove(CacheKeys.Generos);


            TempData["ok"] = "Gênero excluído.";
        }
        catch (DbUpdateException)
        {
            // 2) Caso o banco impeça por FK, ainda assim avisamos
            TempData["error"] = "Exclusão bloqueada pelo banco de dados (há vínculos com filmes).";
        }

        return RedirectToAction(nameof(Index));
    }
}
