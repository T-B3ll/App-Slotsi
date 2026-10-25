using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Mapping;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CitasController : ControllerBase
{
    private readonly ICitaRepository _citas;

    public CitasController(ICitaRepository citas)
    {
        _citas = citas;
    }

    [HttpGet]
    public async Task<ActionResult<List<CitaDto>>> Listar([FromQuery] DateTime? fecha, CancellationToken ct)
    {
        var entidades = fecha.HasValue
            ? await _citas.ObtenerPorFechaAsync(fecha.Value, ct)
            : await _citas.ObtenerTodasAsync(ct);

        return Ok(entidades.Select(c => c.ToDto()).ToList());
    }

    [HttpGet("usuario/{clienteId}")]
    public async Task<ActionResult<List<CitaDto>>> ObtenerPorUsuario(string clienteId, CancellationToken ct)
    {
        if (!IdValidator.EsValido(clienteId)) return BadRequest("ID de cliente inválido");
        var entidades = await _citas.ObtenerPorUsuarioAsync(clienteId, ct);
        return Ok(entidades.Select(c => c.ToDto()).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CitaDto>> ObtenerPorId(string id, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return BadRequest("ID inválido");
        var entidad = await _citas.ObtenerPorIdAsync(id, ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<CitaDto>> Crear(CitaDto dto, CancellationToken ct)
    {
        var entidad = dto.ToEntity();
        await _citas.CrearAsync(entidad, ct);
        var creada = entidad.ToDto();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, CitaDto dto, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return BadRequest("ID inválido");
        var existe = await _citas.ActualizarAsync(dto.ToEntity(id), ct);
        return existe ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return BadRequest("ID inválido");
        var eliminada = await _citas.EliminarAsync(id, ct);
        return eliminada ? NoContent() : NotFound();
    }
}