using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public interface IUsuarioRepository
{
    Task AsegurarIndicesAsync(CancellationToken ct = default);
    Task<List<UsuarioEntity>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<UsuarioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default);
    Task<bool> CrearAsync(UsuarioEntity usuario, CancellationToken ct = default);
    Task<bool> ActualizarAsync(UsuarioEntity usuario, CancellationToken ct = default);
    Task<bool> EliminarAsync(string id, CancellationToken ct = default);

    Task RevocarTokenAsync(string jti, DateTime expiraEn, CancellationToken ct = default);
    Task<bool> TokenRevocadoAsync(string jti, CancellationToken ct = default);
    Task<UsuarioEntity?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default);
    Task<UsuarioEntity?> BuscarPorTextoAsync(string texto, CancellationToken ct = default);




}