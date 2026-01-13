using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Movix.Infrastructure.Persistence;
using Movix.Web.Areas.Admin.Models;
using Movix.Web.Shared;

namespace Movix.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ManagerOrAdmin")]
public class ClassificacoesController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public ClassificacoesController(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;


    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ClassificacaoListFilter f, CancellationToken ct)
    {
        f.Page = Math.Max(1, f.Page);
        f.PageSize = Math.Clamp(f.PageSize, 5, 50);

        var q = _db.Classificacoes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var term = f.Q.Trim();
            q = q.Where(x => x.Nome.Contains(term));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.Nome)
                           .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize)
                           .Select(x => new ClassificacaoRowVm(x.Id, x.Nome))
                           .ToListAsync(ct);

        var model = new PagedClassificacao(f.Page, f.PageSize, total, items);
        ViewBag.Filter = f;
        return View("~/Areas/Admin/Views/Classificacoes/Index.cshtml", model);
    }

    [HttpGet]
    public IActionResult Create() =>
        View("~/Areas/Admin/Views/Classificacoes/Create.cshtml", new ClassificacaoEditVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassificacaoEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("~/Areas/Admin/Views/Classificacoes/Create.cshtml", vm);

        var exists = await _db.Classificacoes.AnyAsync(c => c.Nome == vm.Nome.Trim(), ct);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Nome), "Já existe uma classificação com esse nome.");
            return View("~/Areas/Admin/Views/Classificacoes/Create.cshtml", vm);
        }

        _db.Classificacoes.Add(new Movix.Domain.Entities.Classificacao
        {
            Nome = vm.Nome.Trim()
        });
        await _db.SaveChangesAsync(ct);


        _cache.Remove(CacheKeys.Classificacoes);


        TempData["ok"] = "Classificação criada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var c = await _db.Classificacoes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return NotFound();

        var vm = new ClassificacaoEditVm { Id = c.Id, Nome = c.Nome };
        return View("~/Areas/Admin/Views/Classificacoes/Edit.cshtml", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClassificacaoEditVm vm, CancellationToken ct)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View("~/Areas/Admin/Views/Classificacoes/Edit.cshtml", vm);

        var otherExists = await _db.Classificacoes.AnyAsync(c => c.Id != id && c.Nome == vm.Nome.Trim(), ct);
        if (otherExists)
        {
            ModelState.AddModelError(nameof(vm.Nome), "Já existe outra classificação com esse nome.");
            return View("~/Areas/Admin/Views/Classificacoes/Edit.cshtml", vm);
        }

        var c = await _db.Classificacoes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return NotFound();

        c.Nome = vm.Nome.Trim();
        await _db.SaveChangesAsync(ct);

        _cache.Remove(CacheKeys.Classificacoes);


        TempData["ok"] = "Classificação atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var c = await _db.Classificacoes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return NotFound();
        return View("~/Areas/Admin/Views/Classificacoes/Delete.cshtml", c);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("ADMIN") || User.IsInRole("Administrador");
        if (!isAdmin) return Forbid();

        var c = await _db.Classificacoes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return NotFound();

        var hasFilms = await _db.Filmes.AnyAsync(f => f.ClassificacaoId == id, ct);
        if (hasFilms)
        {
            TempData["error"] = "Não é possível excluir: existem filmes vinculados a esta classificação.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _db.Classificacoes.Remove(c);
            await _db.SaveChangesAsync(ct);

            _cache.Remove(CacheKeys.Classificacoes);


            TempData["ok"] = "Classificação excluída.";
        }
        catch (DbUpdateException)
        {
            TempData["error"] = "Exclusão bloqueada pelo banco de dados (há vínculos com filmes).";
        }

        return RedirectToAction(nameof(Index));
    }



}
