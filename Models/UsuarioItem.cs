namespace slotsi_citas.Models;

public class UsuarioItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Id { get; set; }
    public string IdTexto => $"ID: {Id}";
}