using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Mapping;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos; 

namespace Slotsi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NegociosController : ControllerBase
{
    private readonly INegocioRepository _negocios;

    public NegociosController(INegocioRepository negocios)
    {
        _negocios = negocios;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<NegocioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NegocioDto>>> Listar(CancellationToken ct)
    {
        var entidades = await _negocios.ObtenerTodosAsync(ct);
        return Ok(entidades.Select(e => e.ToDto()).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NegocioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NegocioDto>> ObtenerPorId(string id, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();

        var entidad = await _negocios.ObtenerPorIdAsync(id, ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(NegocioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NegocioDto>> Crear(NegocioDto dto, CancellationToken ct)
    {
        var entidad = dto.ToEntity();
        await _negocios.CrearAsync(entidad, ct);

        var creada = entidad.ToDto();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(string id, NegocioDto dto, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();

        var existe = await _negocios.ActualizarAsync(dto.ToEntity(id), ct);
        return existe ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(string id, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();

        var eliminada = await _negocios.EliminarAsync(id, ct);
        return eliminada ? NoContent() : NotFound();
    }

    private ObjectResult IdInvalido() => Problem(
        title: "Id inválido",
        detail: "El id debe ser un ObjectId de 24 caracteres hexadecimales.",
        statusCode: StatusCodes.Status400BadRequest);
}