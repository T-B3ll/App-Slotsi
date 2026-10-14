using Slotsi.Shared.Dtos;
using System.ComponentModel.DataAnnotations;

namespace slotsi_citas.Models;

public class Usuario
{
    public string? Id { get; set; }

    [Required]
    [StringLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Cedula { get; set; } = string.Empty;

  
    public string Contrasena { get; set; } = string.Empty;

    public bool TipoUsuario { get; set; } = false;

    public bool EstaActivo { get; set; } = true;

  
    public static Usuario FromDto(UsuarioDto dto) => new()
    {
        Id = dto.Id,
        NombreCompleto = dto.NombreCompleto,
        Correo = dto.Correo,
        Telefono = dto.Telefono,
        Cedula = dto.Cedula,
        TipoUsuario = dto.TipoUsuario,
        EstaActivo = dto.EstaActivo
      
    };

   
    public UsuarioDto ToDto() => new()
    {
        Id = Id,
        NombreCompleto = NombreCompleto,
        Correo = Correo,
        Telefono = Telefono,
        Cedula = Cedula,
        TipoUsuario = TipoUsuario,
        EstaActivo = EstaActivo
        
    };
}