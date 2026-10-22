using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Mapping;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CalificacionController : ControllerBase
    {
        private readonly ICalificacionRepository _repo;
        public CalificacionController(ICalificacionRepository repo) => _repo = repo;

        [HttpGet("negocio/{negocioId}")]
        public async Task<ActionResult<List<CalificacionesDtos>>> PorNegocio(string negocioId, CancellationToken ct)
        {
            if (!IdValidator.EsValido(negocioId)) return IdInvalido();
            var lista = await _repo.ObtenerPorNegocioAsync(negocioId, ct);
            return Ok(lista.Select(e => e.ToDto()).ToList());
        }

        [HttpGet("negocio/{negocioId}/promedio")]
        public async Task<ActionResult<PromedioCalificacionDto>> Promedio(string negocioId, CancellationToken ct)
        {
            if (!IdValidator.EsValido(negocioId)) return IdInvalido();
            var (promedio, total) = await _repo.ObtenerPromedioNegocioAsync(negocioId, ct);
            return Ok(new PromedioCalificacionDto { Promedio = Math.Round(promedio, 2), Total = total });
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<CalificacionesDtos>>> PorUsuario(string usuarioId, CancellationToken ct)
        {
            if (!IdValidator.EsValido(usuarioId)) return IdInvalido();
            var lista = await _repo.ObtenerPorUsuarioAsync(usuarioId, ct);
            return Ok(lista.Select(e => e.ToDto()).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<CalificacionesDtos>> Crear(CalificacionesDtos dto, CancellationToken ct)
        {
            if (!IdValidator.EsValido(dto.IdNegocio) || !IdValidator.EsValido(dto.IdUsuarioCliente))
                return IdInvalido();

            var e = dto.ToEntity();
            await _repo.CrearAsync(e, ct);
            return StatusCode(StatusCodes.Status201Created, e.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(string id, CancellationToken ct)
        {
            if (!IdValidator.EsValido(id)) return IdInvalido();
            return await _repo.EliminarAsync(id, ct) ? NoContent() : NotFound();
        }

        private ObjectResult IdInvalido() => Problem(
            title: "Id inválido",
            detail: "El id debe ser un ObjectId de 24 caracteres hexadecimales.",
            statusCode: StatusCodes.Status400BadRequest);
    }
}
