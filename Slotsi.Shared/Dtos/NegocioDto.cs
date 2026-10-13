using System.ComponentModel.DataAnnotations;

namespace Slotsi.Shared.Dtos;

public class NegocioDto
{
    public string? Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string NombreNegocio { get; set; } = string.Empty;

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [StringLength(20)]
    public string NumeroRuc { get; set; } = string.Empty;

    [Required]
    public string DireccionNegocio { get; set; } = string.Empty;

    [Required]
    public string TipoServicio { get; set; } = "Sede";

    public string UrlFotoPerfil { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Dictionary<string, HorarioDiaDto> Horarios { get; set; } = new();
    public List<string> DiasCerrados { get; set; } = new();
}

public class HorarioDiaDto
{
    public string Apertura { get; set; } = "";
    public string Cierre { get; set; } = "";
    public List<string> Pausas { get; set; } = new();
}