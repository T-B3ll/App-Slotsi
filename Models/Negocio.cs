using Slotsi.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace slotsi_citas.Models;

public class Negocio
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string NombreNegocio { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string NumeroRuc { get; set; } = string.Empty;
    public string DireccionNegocio { get; set; } = string.Empty;
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
}