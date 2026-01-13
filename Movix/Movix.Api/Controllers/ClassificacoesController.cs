using Microsoft.AspNetCore.Mvc;
using Movix.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class ClassificacoesController : ControllerBase
{
    private readonly ICatalogLookupService _service;

    public ClassificacoesController(ICatalogLookupService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var classificacoes = await _service.GetClassificacoesAsync();
        return Ok(classificacoes);
    }
}
