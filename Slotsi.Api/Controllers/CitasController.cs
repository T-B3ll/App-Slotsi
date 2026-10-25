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
    public async Task<ActionResult<CitaDto>> Crear([FromBody] CitaDto dto, CancellationToken ct)
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

    [HttpGet("reportes/top-servicios")]
    [ProducesResponseType(typeof(List<TopServicioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TopServicioDto>>> ObtenerTopServicios([FromQuery] string? negocioId, [FromQuery] string? periodo, CancellationToken ct)
    {
        try
        {
            var citas = await _citas.ObtenerTodasAsync(ct);

            if (!string.IsNullOrEmpty(negocioId))
            {
                citas = citas.Where(c => c.NegocioId.ToString() == negocioId).ToList();
            }

            var fechaLimite = DateTime.UtcNow;
            if (periodo == "semana")
            {
                fechaLimite = DateTime.UtcNow.AddDays(-7);
            }
            else if (periodo == "mes")
            {
                fechaLimite = DateTime.UtcNow.AddMonths(-1);
            }

            citas = citas.Where(c => c.FechaHoraInicio >= fechaLimite).ToList();

            var citasValidas = citas
                .Where(c => c.Estado != "Cancelada" && !string.IsNullOrEmpty(c.ServicioNombre))
                .ToList();

            var serviciosDescompuestos = new List<(string Nombre, int Duracion)>();

            foreach (var cita in citasValidas)
            {
                var nombres = cita.ServicioNombre
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                foreach (var nombre in nombres)
                {
                    serviciosDescompuestos.Add((nombre, cita.DuracionMinutos));
                }
            }

            var topServicios = serviciosDescompuestos
                .GroupBy(s => s.Nombre)
                .Select(g => new TopServicioDto
                {
                    NombreServicio = g.Key,
                    TotalCitas = g.Count(),
                    IngresosTotales = 0,
                    DuracionMinutos = (int)g.Average(x => x.Duracion) 
                })
                .OrderByDescending(x => x.TotalCitas)
                .Take(5)
                .ToList();

            return Ok(topServicios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("reportes/demanda-semanal")]
    [ProducesResponseType(typeof(List<DemandaSemanalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DemandaSemanalDto>>> ObtenerDemandaSemanal([FromQuery] string? negocioId, [FromQuery] string? periodo, CancellationToken ct)
    {
        try
        {
            var citas = await _citas.ObtenerTodasAsync(ct);

            if (!string.IsNullOrEmpty(negocioId))
            {
                citas = citas.Where(c => c.NegocioId.ToString() == negocioId).ToList();
            }

            var fechaLimite = DateTime.UtcNow;
            if (periodo == "semana")
            {
                fechaLimite = DateTime.UtcNow.AddDays(-7);
            }
            else if (periodo == "mes")
            {
                fechaLimite = DateTime.UtcNow.AddMonths(-1);
            }

            citas = citas.Where(c => c.FechaHoraInicio >= fechaLimite).ToList();

            var demanda = citas
                .Where(c => c.Estado != "Cancelada")
                .GroupBy(c => c.FechaHoraInicio.DayOfWeek)
                .Select(g => new DemandaSemanalDto
                {
                    DiaSemana = g.Key.ToString(),
                    TotalCitas = g.Count()
                })
                .OrderBy(x => x.DiaSemana)
                .ToList();

            return Ok(demanda);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}