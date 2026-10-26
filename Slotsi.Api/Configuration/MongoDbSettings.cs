using System.ComponentModel.DataAnnotations;

namespace Slotsi.Api.Configuration;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    [Required] public string ConnectionString { get; set; } = string.Empty;
    [Required] public string DatabaseName { get; set; } = string.Empty;
    [Required] public string UsuariosCollection { get; set; } = "Usuarios";
    [Required] public string NegociosCollection { get; set; } = "Negocios";
    [Required] public string CitasCollection { get; set; } = "Citas";
    [Required] public string TrabajadoresCollection { get; set; } = "Trabajadores";
    [Required] public string ServiciosCollection { get; set; } = "servicios";
    [Required] public string CalificacionesCollection { get; set; } = "Calificaciones";
    [Required] public string HistorialPagosCollection { get; set; } = "HistorialPagos";
    [Required] public string PagoSubCollection { get; set; } = "PagoSub";
    [Required] public string RecuperarContrasenaCollection { get; set; } = "RecuperarContrasena";

    [Required]
    public string TokensRevocadosCollection { get; set; } = "tokens_revocados";
}