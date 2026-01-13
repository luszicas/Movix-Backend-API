//refatorado fase 4

using Microsoft.AspNetCore.Mvc;

using Movix.Web.Models;         // ViewModels/DTOs usados na camada Web (CatalogFilterVm, CatalogPageVm, etc).
using Movix.Web.Services;       // IApiClient: cliente HTTP para consumir a API do backend.

namespace Movix.Web.Controllers;    // Namespace da camada Web (MVC).

// Controller com primary-constructor: injeta IApiClient e ILogger<HomeController>.
public class HomeController(IApiClient api, ILogger<HomeController> log) : Controller
{
    private readonly IApiClient _api = api;   // Mant?m refer?ncia ao cliente da API para chamadas de dados.
    // Observa??o: 'log' est? dispon?vel via par?metro, mas n?o est? armazenado em um campo; use diretamente se necess?rio.

    // Monta o ViewModel da p?gina do cat?logo, normalizando pagina??o e preenchendo listas auxiliares.
    private async Task<CatalogPageVm> BuildVm(CatalogFilterVm filter, CancellationToken ct)
    {
        if (filter.Page < 1) filter.Page = 1;               // Saneamento: p?gina m?nima ? 1.

        // Agora suportamos filter.PageSize == 0 como sinal "trazer todos os filmes".
        if (filter.PageSize < 0) filter.PageSize = 0;       // normaliza negativos para 0 (tudo).
        if (filter.PageSize > 2000) filter.PageSize = 2000; // limite superior de segurança.

        return new CatalogPageVm
        {
            Filter = filter,                                // Ret?m filtros atuais (para reflex?o na UI).
            Generos = await _api.GetGenerosAsync(ct),             // Preenche <select> de g?neros (com cache no ApiClient).
            Classificacoes = await _api.GetClassificacoesAsync(ct),   // Preenche <select> de classifica??es (cache idem).
            PageResult = await _api.SearchFilmesAsync(filter, ct)     // Consulta (agora pode retornar todos se pageSize==0).
        };
    }

    // ONE-PAGE: /  (aceita filtros via querystring)
    [HttpGet("")]                                           // Rota raiz do site.
    public async Task<IActionResult> Index([FromQuery] CatalogFilterVm filter, string? section, CancellationToken ct)
    {
        var vm = await BuildVm(filter, ct);               // Constr?i o ViewModel com dados + filtros.
        ViewBag.Section = section; // "about" | "genres" | "catalog"
        return View(vm);
    }

    // Compat: /Home/Catalog -> Index em #catalog
    [HttpGet]                                               // GET /Home/Catalog
    public Task<IActionResult> Catalog([FromQuery] CatalogFilterVm filter, CancellationToken ct)
        => Index(filter, "catalog", ct);                        // Reusa Index e ativa se??o "catalog".

    // Compat: /Home/Genre -> Index em #genres
    [HttpGet]                                               // GET /Home/Genre
    public Task<IActionResult> Genre(CancellationToken ct)
        => Index(new CatalogFilterVm(), "genres", ct);            // Reusa Index com filtros default e se??o "genres".

    [HttpGet("/Home/Planos")]
    public IActionResult Planos()
    {
        return View(); // Renderiza a View Views/Home/Planos.cshtml
    }

    /// <summary>
    /// Renderiza a p?gina "Filmes" (Views/Home/Filmes.cshtml) solicitando todos os filmes.
    /// </summary>
    [HttpGet("/Home/Filmes")]
    public async Task<IActionResult> Filmes([FromQuery] CatalogFilterVm filter, CancellationToken ct)
    {
        // Forçamos pageSize = 0 para pedir "todos os filmes" (repositório interpreta 0 como 'no pagination').
        filter.Page = 1;
        filter.PageSize = 0;

        var vm = await BuildVm(filter, ct);
        return View(vm);
    }
}