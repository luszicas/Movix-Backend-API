using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Movix.Web.Areas.Admin.Services;

namespace Movix.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : Controller
{
    private readonly DashboardService _svc;

    public DashboardController(DashboardService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var totalFilmes = await _svc.TotalFilmesAsync(ct);
        var totalGeneros = await _svc.TotalGenerosAsync(ct);
        var totalUsuarios = await _svc.TotalUsuariosAsync(ct);
        var porRole = await _svc.TotalUsuariosPorRoleAsync(ct);
        var ultimos = await _svc.UltimosFilmesAsync(5, ct);

        ViewBag.TotalFilmes = totalFilmes;
        ViewBag.TotalGeneros = totalGeneros;
        ViewBag.TotalUsuarios = totalUsuarios;
        ViewBag.PorRole = porRole;
        ViewBag.Ultimos = ultimos;

        return View("~/Areas/Admin/Views/Dashboard/Index.cshtml");
    }

    // Endpoint de dados para os gráficos (JSON)
    [HttpGet("Admin/Dashboard/Data")]
    public async Task<IActionResult> Data(CancellationToken ct)
    {
        var porGenero = await _svc.FilmesPorGeneroAsync(ct);
        var porClass = await _svc.FilmesPorClassificacaoAsync(ct);
        var porAno = await _svc.FilmesPorAnoAsync(ct);

        return Json(new
        {
            porGenero = new
            {
                labels = porGenero.Select(x => x.Label).ToArray(),
                data = porGenero.Select(x => x.Qtd).ToArray()
            },
            porClass = new
            {
                labels = porClass.Select(x => x.Label).ToArray(),
                data = porClass.Select(x => x.Qtd).ToArray()
            },
            porAno = new
            {
                labels = porAno.Select(x => x.Ano).ToArray(),
                data = porAno.Select(x => x.Qtd).ToArray()
            }
        });
    }
}
