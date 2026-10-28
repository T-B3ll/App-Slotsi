using Slotsi.Shared.Dtos;
using slotsi_citas.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Services.Api;
using Slotsi.Shared.Dtos;
using System.Net;


namespace slotsi_citas.Services;

public class UsuarioService : ApiServiceBase
{

    public UsuarioService(HttpClient http) : base(http) { }

   
    public async Task<Usuario?> ObtenerPorIdentificadorAsync(string identificador)
    {
        try
        {
            var dto = await GetOrNullAsync<UsuarioDto>($"/api/usuarios/buscar/{Uri.EscapeDataString(identificador)}");
            return dto != null ? Usuario.FromDto(dto) : null;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }


    public async Task CrearAsync(Usuario usuario)
    {
      
        await PostAsync<UsuarioDto>("/api/usuarios", usuario.ToDto());
    }


    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        try
        {
            var dto = await GetOrNullAsync<UsuarioDto>($"/api/usuarios/correo/{Uri.EscapeDataString(correo)}");
            return dto != null ? Usuario.FromDto(dto) : null;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

  
    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        var dtos = await GetAsync<List<UsuarioDto>>("/api/usuarios");
        return dtos.Select(d => Usuario.FromDto(d)).ToList();
    }

   
    public async Task ActualizarAsync(Usuario usuario)
    {
        await PutAsync($"/api/usuarios/{usuario.Id}", usuario.ToDto());
    }


    public async Task CambiarEstadoAsync(string id, bool estaActivo)
    {
        await PatchAsync($"/api/usuarios/{id}/estado", new { EstaActivo = estaActivo });
    }


    public async Task ActualizarContrasenaAsync(string correo, string nuevaContrasenaHash)
    {
        await PatchAsync($"/api/usuarios/password", new { Correo = correo, Contrasena = nuevaContrasenaHash });
    }


    public async Task<Usuario?> ObtenerPorIdAsync(string id)
    {
        try
        {
            var dto = await GetOrNullAsync<UsuarioDto>($"/api/usuarios/{id}");
            return dto != null ? Usuario.FromDto(dto) : null;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}