using System.Threading.Tasks;
using Slotsi.Shared.Dtos;

namespace slotsi_citas.Services.Api
{
    public interface IAuthApiService
    {
        Task RegistrarAsync(RegistrarUsuarioDto dto);
        Task IniciarSesionAsync(string email, string contrasena);
        Task CerrarSesionAsync();
        Task<SolicitudRestablecimientoRespuestaDto> SolicitarRestablecimientoAsync(string email);
        Task RestablecerContrasenaAsync(RestablecerContrasenaDto dto);
        Task ValidarTokenAsync(string email, string token);
    }
}