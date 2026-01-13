using Microsoft.AspNetCore.Mvc;
using Movix.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class GenerosController : ControllerBase
{
    private readonly ICatalogLookupService _service;

    public GenerosController(ICatalogLookupService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var generos = await _service.GetGenerosAsync();
        return Ok(generos);
    }
}
