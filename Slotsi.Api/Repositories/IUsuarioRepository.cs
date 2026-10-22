using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public interface IUsuarioRepository
{
    Task<List<UsuarioEntity>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<UsuarioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default);
    Task CrearAsync(UsuarioEntity usuario, CancellationToken ct = default);
    Task<bool> ActualizarAsync(UsuarioEntity usuario, CancellationToken ct = default);
    Task<bool> EliminarAsync(string id, CancellationToken ct = default);


    Task<UsuarioEntity?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default);
    Task<UsuarioEntity?> BuscarPorTextoAsync(string texto, CancellationToken ct = default);




}