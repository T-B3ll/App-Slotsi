using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Mapping;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Slotsi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarios;

    public UsuariosController(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioDto>>> Listar(CancellationToken ct)
    {
        var entidades = await _usuarios.ObtenerTodosAsync(ct);
        return Ok(entidades.Select(e => e.ToDto()).ToList());
    }


    [HttpGet("correo/{correo}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorCorreo(string correo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return BadRequest(new ProblemDetails { Detail = "El correo no puede estar vacío." });

        var entidad = await _usuarios.ObtenerPorCorreoAsync(correo.ToLower().Trim(), ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

    [HttpGet("buscar/{identificador}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> BuscarPorIdentificador(string identificador, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(identificador))
            return BadRequest(new ProblemDetails { Detail = "El identificador no puede estar vacío." });

        var entidad = await _usuarios.BuscarPorTextoAsync(identificador.Trim(), ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

   
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(string id, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();

        var entidad = await _usuarios.ObtenerPorIdAsync(id, ct);
        return entidad is null ? NotFound() : Ok(entidad.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> Crear(UsuarioDto dto, CancellationToken ct)
    {
        System.Diagnostics.Debug.WriteLine($"[API-CREAR] DTO recibido - Contrasena: '{dto.Contrasena}'");
        System.Diagnostics.Debug.WriteLine($"[API-CREAR] DTO recibido - Nombre: '{dto.NombreCompleto}'");

        var entidad = dto.ToEntity();

        System.Diagnostics.Debug.WriteLine($"[API-CREAR] Entity después de mapeo - Contrasena: '{entidad.Contrasena}'");

        await _usuarios.CrearAsync(entidad, ct);

        var creada = entidad.ToDto();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }


    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(string id, UsuarioDto dto, CancellationToken ct)
    {
        if (!IdValidator.EsValido(id)) return IdInvalido();

        var existe = await _usuarios.ActualizarAsync(dto.ToEntity(id), ct);
        return existe ? NoContent() : NotFound();
    }

   
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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