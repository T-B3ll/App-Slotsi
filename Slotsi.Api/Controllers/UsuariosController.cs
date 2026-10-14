using Microsoft.AspNetCore.Mvc;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Mapping;
using Slotsi.Api.Repositories;
using Slotsi.Shared.Dtos; 

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

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioDto>>> Listar(CancellationToken ct)
    {
        var entidades = await _usuarios.ObtenerTodosAsync(ct);
        return Ok(entidades.Select(e => e.ToDto()).ToList());
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
        var entidad = dto.ToEntity(); 
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