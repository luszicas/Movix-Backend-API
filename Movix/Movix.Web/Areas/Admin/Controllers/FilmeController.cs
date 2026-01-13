using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movix.Domain.Entities;
using Movix.Infrastructure.Persistence;
using Movix.Web.Areas.Admin.Models;


namespace Movix.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FilmesController : Controller
    {
        private readonly AppDbContext _context;

        public FilmesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Filmes
        public async Task<IActionResult> Index()
        {
            var filmes = await _context.Filmes
                .Include(f => f.Genero)
                .Include(f => f.Classificacao)
                .ToListAsync();
            return View(filmes);
        }

        // GET: Admin/Filmes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var filme = await _context.Filmes
                .Include(f => f.Genero)
                .Include(f => f.Classificacao)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (filme == null)
                return NotFound();

            var vm = new FilmeEditVm
            {
                Id = filme.Id,
                Titulo = filme.Titulo,
                Ano = filme.Ano,
                Sinopse = filme.Sinopse,
                ImagemCapaUrl = filme.ImagemCapaUrl,
                UrlTrailer = filme.UrlTrailer,
                GeneroId = filme.GeneroId,
                ClassificacaoId = filme.ClassificacaoId
            };

            ViewBag.GeneroNome = filme.Genero?.Nome ?? "-";
            ViewBag.ClassificacaoNome = filme.Classificacao?.Nome ?? "-";

            return View(vm);
        }

        // GET: Admin/Filmes/Create
        public IActionResult Create()
        {
            CarregarListas();
            return View(new FilmeEditVm());
        }

        // POST: Admin/Filmes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FilmeEditVm vm, IFormFile? capaArquivo)
        {
            if (ModelState.IsValid)
            {
                var filme = new Filme
                {
                    Titulo = vm.Titulo,
                    Ano = vm.Ano,
                    Sinopse = vm.Sinopse,
                    ImagemCapaUrl = vm.ImagemCapaUrl,
                    UrlTrailer = vm.UrlTrailer,
                    GeneroId = vm.GeneroId,
                    ClassificacaoId = vm.ClassificacaoId
                };

                if (capaArquivo != null && capaArquivo.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(capaArquivo.FileName);
                    var path = Path.Combine("wwwroot", "uploads", fileName);
                    using (var stream = System.IO.File.Create(path))
                        await capaArquivo.CopyToAsync(stream);
                    filme.ImagemCapaUrl = "/uploads/" + fileName;
                }

                _context.Add(filme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CarregarListas();
            return View(vm);
        }

        // GET: Admin/Filmes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var filme = await _context.Filmes.FindAsync(id);
            if (filme == null) return NotFound();

            var vm = new FilmeEditVm
            {
                Id = filme.Id,
                Titulo = filme.Titulo,
                Ano = filme.Ano,
                Sinopse = filme.Sinopse,
                ImagemCapaUrl = filme.ImagemCapaUrl,
                UrlTrailer = filme.UrlTrailer,
                GeneroId = filme.GeneroId,
                ClassificacaoId = filme.ClassificacaoId
            };

            CarregarListas();
            return View(vm);
        }

        // POST: Admin/Filmes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FilmeEditVm vm, IFormFile? capaArquivo)
        {
            if (id != vm.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var filme = await _context.Filmes.FindAsync(id);
                if (filme == null) return NotFound();

                filme.Titulo = vm.Titulo;
                filme.Ano = vm.Ano;
                filme.Sinopse = vm.Sinopse;
                filme.GeneroId = vm.GeneroId;
                filme.ClassificacaoId = vm.ClassificacaoId;
                filme.UrlTrailer = vm.UrlTrailer;

                if (capaArquivo != null && capaArquivo.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(capaArquivo.FileName);
                    var path = Path.Combine("wwwroot", "uploads", fileName);
                    using (var stream = System.IO.File.Create(path))
                        await capaArquivo.CopyToAsync(stream);
                    filme.ImagemCapaUrl = "/uploads/" + fileName;
                }
                else
                {
                    filme.ImagemCapaUrl = vm.ImagemCapaUrl;
                }

                _context.Update(filme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CarregarListas();
            return View(vm);
        }

        // GET: Admin/Filmes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var filme = await _context.Filmes
                .Include(f => f.Genero)
                .Include(f => f.Classificacao)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (filme == null)
                return NotFound();

            return View(filme);
        }

        // POST: Admin/Filmes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var filme = await _context.Filmes.FindAsync(id);
            if (filme != null)
            {
                _context.Filmes.Remove(filme);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void CarregarListas()
        {
            ViewBag.Generos = _context.Generos
                .OrderBy(g => g.Nome)
                .Select(g => new { g.Id, g.Nome })
                .ToList();

            ViewBag.Classificacoes = _context.Classificacoes
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Id, c.Nome })
                .ToList();
        }

       

    }
}
