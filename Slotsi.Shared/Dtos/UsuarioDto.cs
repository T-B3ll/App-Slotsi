using System.ComponentModel.DataAnnotations;

namespace Slotsi.Shared.Dtos;

public class UsuarioDto
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(20)]
    public string Cedula { get; set; } = string.Empty;
    [StringLength(100)]
    public string Contrasena { get; set; } = string.Empty;


    public bool TipoUsuario { get; set; } = false;
    public bool EstaActivo { get; set; } = true;
}   