using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Mapping;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Slotsi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarios;

    public UsuariosController(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioDto>>> Listar(CancellationToken ct)
    {
        var entidades = await _usuarios.ObtenerTodosAsync(ct);
        return Ok(entidades.Select(e => e.ToDto()).ToList());
    }

    [AllowAnonymous]
    [HttpGet("correo/{correo}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorCorreo(string correo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return BadRequest(new ProblemDetails { Detail = "El correo no puede estar vacío." });

        var entidad = await _usuarios.ObtenerPorCorreoAsync(correo.ToLower().Trim(), ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

    [AllowAnonymous]
    [HttpGet("buscar/{identificador}")]
    public async Task<ActionResult<UsuarioDto>> BuscarPorIdentificador(string identificador, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(identificador))
            return BadRequest(new ProblemDetails { Detail = "El identificador no puede estar vacío." });

        var entidad = await _usuarios.BuscarPorTextoAsync(identificador.Trim(), ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(string id, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();
        var entidad = await _usuarios.ObtenerPorIdAsync(id, ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(UsuarioDto dto, CancellationToken ct)
    {
        var entidad = dto.ToEntity();
        await _usuarios.CrearAsync(entidad, ct);
        var creada = entidad.ToDto();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, UsuarioDto dto, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();
        var existe = await _usuarios.ActualizarAsync(dto.ToEntity(id), ct);
        return existe ? NoContent() : NotFound();
    }

    // --- NUEVO ENDPOINT PARA ACTIVAR/INACTIVAR ---
    [Authorize]
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(string id, [FromBody] CambiarEstadoRequest request, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();

        // Intentamos actualizar el estado. Si el repositorio no tiene el método, tendrás que agregarlo.
        // Si lo tiene, esto funcionará perfecto.
        var actualizado = await _usuarios.CambiarEstadoAsync(id, request.EstaActivo, ct);

        return actualizado ? NoContent() : NotFound();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();
        var eliminada = await _usuarios.EliminarAsync(id, ct);
        return eliminada ? NoContent() : NotFound();
    }

    private ObjectResult IdInvalido() => Problem(
        title: "Id inválido",
        detail: "El id debe ser un ObjectId de 24 caracteres hexadecimales.",
        statusCode: StatusCodes.Status400BadRequest);
}


public class CambiarEstadoRequest
{
    public bool EstaActivo { get; set; }
}