using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SuscripcionController : ControllerBase
{
    private readonly ISuscripcionRepository _suscripciones;

    public SuscripcionController(ISuscripcionRepository suscripciones)
    {
        _suscripciones = suscripciones;
    }

    [HttpGet("activa/{usuarioId}")]
    public async Task<ActionResult<SuscripcionActivaDto>> ObtenerActiva(string usuarioId, CancellationToken ct)
    {
        var sub = await _suscripciones.ObtenerSuscripcionActivaAsync(usuarioId, ct);
        if (sub is null) return NotFound();
        return Ok(new SuscripcionActivaDto
        {
            Id = sub.Id.ToString(),
            UsuarioId = sub.UsuarioId,
            Estado = sub.Estado,
            FechaUltimoPago = sub.FechaUltimoPago,
            ProximoVencimiento = sub.ProximoVencimiento,
            MontoPagado = sub.MontoPagado
        });
    }

    [HttpPost("pagar")]
    public async Task<IActionResult> Pagar(PagoDto dto, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dto.UsuarioId)) return BadRequest("UsuarioId es requerido.");
        if (dto.Monto <= 0) return BadRequest("El monto debe ser mayor a cero.");

        await _suscripciones.ProcesarPagoAsync(dto.UsuarioId, dto.Monto, dto.MetodoPago, ct);
        return Ok(new { mensaje = "Pago procesado correctamente." });
    }
}