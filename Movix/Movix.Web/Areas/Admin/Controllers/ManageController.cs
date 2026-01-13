//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Movix.Web.Areas.Admin.Controllers;

//[Area("Admin")]
//[Authorize(Policy = "ManagerOrAdmin")]
//public class ManageController : Controller
//{
//    [HttpGet]
//    public IActionResult Index()
//        => View();

//    // Placeholders para os CRUDs (Fase 7 vai implementar)
//    [HttpGet]
//    public IActionResult Filmes()
//        => View();

//    [HttpGet]
//    public IActionResult Generos()
//        => View();

//    [HttpGet]
//    public IActionResult Classificacoes()
//        => View();

//    [HttpGet]
//    public IActionResult Usuarios()
//        => View();
//}

//Refatorado fase 7

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Movix.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ManagerOrAdmin")]
public class ManageController : Controller
{
    [HttpGet] public IActionResult Index() => View();

    [HttpGet] public IActionResult Filmes() => RedirectToAction("Index", "Filmes", new { area = "Admin" });
    [HttpGet] public IActionResult Generos() => RedirectToAction("Index", "Generos", new { area = "Admin" });
    [HttpGet] public IActionResult Classificacoes() => RedirectToAction("Index", "Classificacoes", new { area = "Admin" });

    [HttpGet] public IActionResult Usuarios() => RedirectToAction("Index", "Usuarios", new { area = "Admin" });
}
