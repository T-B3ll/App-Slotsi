using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slotsi.Shared.Dtos;


namespace slotsi_citas.Services.Api
{
    public interface IAuthApiService
    {
        Task RegistrarAsync(RegistrarUsuarioDto dto);

        //pide el tokenb a la api y lo guarda en la sesion
        Task IniciarSesionAsync(string email, string contrasena);

        Task CerrarSesionAsync();


        //en development la API devuelve el token en la respuesta (no hay correo)
        Task<SolicitudRestablecimientoRespuestaDto> SolicitarRestablecimientoAsync(string email);

        Task RestablecerContrasenaAsync(RestablecerContrasenaDto dto);


    }
}
