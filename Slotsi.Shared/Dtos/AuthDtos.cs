using System.ComponentModel.DataAnnotations;

namespace Slotsi.Shared.Dtos
{
    public class RegistrarUsuarioDto
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(100, MinimumLength = 8)]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class TokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
    }

    public class SolicitarRestablecimientoDto
    {
        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;
    }

    public class RestablecerContrasenaDto
    {
        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;


        [Required, StringLength(100, MinimumLength = 8)]
        public string NuevaContrasena { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class SolicitudRestablecimientoRespuestaDto
    {
        public string Mensaje { get; set; } = string.Empty;

        public string? Token { get; set; }
    }
}
