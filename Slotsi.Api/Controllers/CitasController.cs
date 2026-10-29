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
    public async Task<ActionResult<CitaDto>> Crear([FromBody] CitaDto dto, CancellationToken ct)
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

    [HttpGet("reportes/cancelaciones")]
    [ProducesResponseType(typeof(CancelacionesResporteDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CancelacionesResporteDto>> ObtenerCancelaciones(
    [FromQuery] string negocioId,
    [FromQuery] DateTime desde,
    [FromQuery] DateTime hasta,
    CancellationToken ct)
    {
        if (!IdValidator.EsValido(negocioId)) return BadRequest("ID de negocio inválido");
        if (hasta < desde) return BadRequest("'hasta' no puede ser anterior a 'desde'");

        var citas = await _citas.ObtenerPorRangoConCanceladasAsync(negocioId, desde, hasta, ct);

        static bool EsCancelada(string? e) =>
            e is not null && e.StartsWith("cancelad", StringComparison.OrdinalIgnoreCase);

        static bool EsNoAsistio(string? e) =>
            e is not null && e.Replace(" ", "").StartsWith("noasist", StringComparison.OrdinalIgnoreCase);

        var total = citas.Count;
        var canceladas = citas.Count(c => EsCancelada(c.Estado));
        var noAsistio = citas.Count(c => EsNoAsistio(c.Estado));

        static double Pct(int parte, int todo) =>
            todo == 0 ? 0 : Math.Round(parte * 100.0 / todo, 1);

        var dto = new CancelacionesResporteDto
        {
            Desde = desde,
            Hasta = hasta,
            Total = total,
            Canceladas = canceladas,
            NoAsistio = noAsistio,
            PorcentajeCancelacion = Pct(canceladas, total),
            PorcentajeNoAsistio = Pct(noAsistio, total),
            PorcentajePerdida = Pct(canceladas + noAsistio, total),
            Citas = citas
                .Where(c => EsCancelada(c.Estado) || EsNoAsistio(c.Estado))
                .Select(c => c.ToDto())
                .ToList()
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

        
            var hoyLocal = DateTime.Now.Date; 
            int diasParaLunes = ((int)hoyLocal.DayOfWeek + 6) % 7;
            var inicioSemana = hoyLocal.AddDays(-diasParaLunes);
            var finSemana = inicioSemana.AddDays(7);

           
            citas = citas.Where(c =>
            {
                var fechaLocal = c.FechaHoraInicio.ToLocalTime();
                return fechaLocal >= inicioSemana && fechaLocal < finSemana;
            }).ToList();

            var demanda = citas
                .Where(c => c.Estado != "Cancelada")
                .GroupBy(c => new
                {
                    Dia = c.FechaHoraInicio.ToLocalTime().DayOfWeek,
                    Hora = c.FechaHoraInicio.ToLocalTime().Hour
                })
                .Select(g => new DemandaSemanalDto
                {
                    DiaSemana = g.Key.Dia.ToString(),
                    Hora = g.Key.Hora,
                    TotalCitas = g.Count()
                })
                .ToList();

            return Ok(demanda);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}