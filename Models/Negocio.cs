using Slotsi.Shared.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace slotsi_citas.Models;

public class ObjectIdToStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            if (doc.RootElement.TryGetProperty("$oid", out var oidElement))
                return oidElement.GetString() ?? string.Empty;
            return doc.RootElement.GetRawText();
        }
        return reader.GetString() ?? string.Empty;
    }


    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public class Negocio
{
    
    [JsonConverter(typeof(ObjectIdToStringConverter))]
    public string Id { get; set; } = string.Empty;

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
    public string UrlDocumentoTitulo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public Dictionary<string, HorarioDia> Horarios { get; set; } = new();
    public List<string> DiasCerrados { get; set; } = new();

    public class HorarioDia
    {
        public string Apertura { get; set; } = "";
        public string Cierre { get; set; } = "";
        public List<string> Pausas { get; set; } = new();
    }

    public static Negocio FromDto(NegocioDto dto) => new()
    {
        Id = dto.Id,
        Nombre = dto.Nombre,
        NombreNegocio = dto.NombreNegocio,
        UsuarioId = dto.UsuarioId,
        NumeroRuc = dto.NumeroRuc,
        DireccionNegocio = dto.DireccionNegocio,
        TipoServicio = dto.TipoServicio,
        UrlFotoPerfil = dto.UrlFotoPerfil,
        UrlDocumentoTitulo = dto.UrlDocumentoTitulo,
        Categoria = dto.Categoria,
        FechaRegistro = dto.FechaRegistro.Kind == DateTimeKind.Utc
            ? dto.FechaRegistro.ToLocalTime()
            : dto.FechaRegistro,
        Horarios = dto.Horarios.ToDictionary(
            kvp => kvp.Key,
            kvp => new HorarioDia
            {
                Apertura = kvp.Value.Apertura,
                Cierre = kvp.Value.Cierre,
                Pausas = kvp.Value.Pausas
            }),
        DiasCerrados = dto.DiasCerrados.ToList()
    };

    public NegocioDto ToDto() => new()
    {
        Id = Id,
        Nombre = Nombre,
        NombreNegocio = NombreNegocio,
        UsuarioId = UsuarioId,
        NumeroRuc = NumeroRuc,
        DireccionNegocio = DireccionNegocio,
        TipoServicio = TipoServicio,
        UrlFotoPerfil = UrlFotoPerfil,
        UrlDocumentoTitulo = UrlDocumentoTitulo,
        Categoria = Categoria,
        FechaRegistro = FechaReporteUtc(),
        Horarios = Horarios.ToDictionary(
            kvp => kvp.Key,
            kvp => new HorarioDiaDto
            {
                Apertura = kvp.Value.Apertura,
                Cierre = kvp.Value.Cierre,
                Pausas = kvp.Value.Pausas
            }),
        DiasCerrados = DiasCerrados.ToList()
    };

    private DateTime FechaReporteUtc() =>
        FechaRegistro.Kind == DateTimeKind.Utc
            ? FechaRegistro
            : FechaRegistro.ToUniversalTime();
}