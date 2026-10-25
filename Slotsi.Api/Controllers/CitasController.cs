using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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
    private readonly INegocioRepository _negocio;

    public CitasController(ICitaRepository citas, INegocioRepository negocio)
    {
        _citas = citas;
        _negocio = negocio;
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

    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenDiaDto>> Resumen(
    [FromQuery] string negocioId,
    [FromQuery] DateTime desde,
    [FromQuery] DateTime hasta,
    CancellationToken ct)
    {
        if (!IdValidator.EsValido(negocioId)) return BadRequest("ID de negocio inválido");
        if (hasta < desde) return BadRequest("'hasta' no puede ser anterior a 'desde'");

        var citas = await _citas.ObtenerPorRangoAsync(negocioId, desde, hasta, ct);

        var negocio = await _negocio.ObtenerPorIdAsync(negocioId, ct);

        var trabajadores = Math.Max(1, citas.Select(c => c.TrabajadorId)
                                            .Where(t => t != ObjectId.Empty)
                                            .Distinct().Count());

        var minutosReservados = citas.Sum(c => c.DuracionMinutos);

        var minutosPorTrabajador = negocio is null
            ? 0
            : OcupacionCalculator.MinutosDisponiblesPorTrabajador(negocio, desde, hasta);

        var minutosDisponibles = minutosPorTrabajador * trabajadores;

        var ocupacion = minutosDisponibles == 0
            ? 0
            : Math.Min(100, (int)Math.Round(minutosReservados * 100.0 / minutosDisponibles));

        var hoy = DateTime.Today;
        var serviciosHoy = citas
           .Where(c => c.FechaHoraInicio.Date == hoy && c.ServicioId != ObjectId.Empty)
           .Select(c => c.ServicioId)
           .Distinct()
           .Count();

        var dto = new ResumenDiaDto
        {
            Desde = desde.Date,
            Hasta = hasta.Date,
            CitasConfirmadas = citas.Count(c => c.Estado == "Confirmada"),
            CitasPendientes = citas.Count(c => c.Estado == "Pendiente"),
            OcupacionPorcentaje = ocupacion,
            ServiciosActivosHoy = serviciosHoy,
            Citas = citas.Select(c => c.ToDto()).ToList()
        };

        return Ok(dto);
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