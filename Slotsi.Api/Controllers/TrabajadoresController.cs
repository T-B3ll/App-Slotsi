using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Mapping;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Slotsi.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TrabajadoresController : ControllerBase
    {
        private readonly ITrabajadorRepository _repo;
        public TrabajadoresController(ITrabajadorRepository repo) => _repo = repo;

        [HttpGet("negocio/{negocioId}")]
        public async Task<ActionResult<List<TrabajadorDto>>> PorNegocio(string negocioId, CancellationToken ct)
        {
            if (!IdValidator.EsValido(negocioId)) return IdInvalido();
            var lista = await _repo.ObtenerPorNegocioAsync(negocioId, ct);
            return Ok(lista.Select(e => e.ToDto()).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrabajadorDto>> ObtenerPorId(string id, CancellationToken ct)
        {
            if (!IdValidator.EsValido(id)) return IdInvalido();
            var e = await _repo.ObtenerPorIdAsync(id, ct);
            return e is null ? NotFound() : Ok(e.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<TrabajadorDto>> Crear(TrabajadorDto dto, CancellationToken ct)
        {
            if (!IdValidator.EsValido(dto.NegocioId)) return IdInvalido();
            var e = dto.ToEntity();
            await _repo.CrearAsync(e, ct);
            var creada = e.ToDto();
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(string id, TrabajadorDto dto, CancellationToken ct)
        {
            if (!IdValidator.EsValido(id) || !IdValidator.EsValido(dto.NegocioId)) return IdInvalido();
            return await _repo.ActualizarAsync(dto.ToEntity(id), ct) ? NoContent() : NotFound();
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(string id, CambiarEstadoDto dto, CancellationToken ct)
        {
            if (!IdValidator.EsValido(id)) return IdInvalido();
            return await _repo.CambiarEstadoAsync(id, dto.Activo, ct) ? NoContent() : NotFound();
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
