using System.Net;
using System.Net.Http.Json;

namespace slotsi_citas.Services.Api;

public abstract class ApiServiceBase
{
    protected readonly HttpClient _http;

    protected ApiServiceBase(HttpClient http)
    {
        _http = http;
    }

 
    protected async Task<T> GetAsync<T>(string url)
    {
        using var respuesta = await EnviarAsync(() => _http.GetAsync(url));
        return await LeerCuerpoAsync<T>(respuesta);
    }

    protected async Task<T?> GetOrNullAsync<T>(string url) where T : class
    {
        using var respuesta = await EnviarAsync(() => _http.GetAsync(url), permitirNotFound: true);
        return respuesta.StatusCode == HttpStatusCode.NotFound ? null : await LeerCuerpoAsync<T>(respuesta);
    }


    protected async Task PostAsync(string url, object cuerpo)
    {
        using var respuesta = await EnviarAsync(() => _http.PostAsJsonAsync(url, cuerpo));
    }

    protected async Task PutAsync(string url, object cuerpo)
    {
        using var respuesta = await EnviarAsync(() => _http.PutAsJsonAsync(url, cuerpo));
    }

    protected async Task PatchAsync(string url, object cuerpo)
    {
        using var respuesta = await EnviarAsync(() => _http.PatchAsJsonAsync(url, cuerpo));
    }

    protected async Task DeleteAsync(string url)
    {
        using var respuesta = await EnviarAsync(() => _http.DeleteAsync(url));
    }

    private static async Task<HttpResponseMessage> EnviarAsync(
        Func<Task<HttpResponseMessage>> envio, bool permitirNotFound = false)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await envio();
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiException("El servidor tardó demasiado en responder.", null, ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("No se pudo conectar con el servidor. Verifica tu red.", null, ex);
        }

        if (respuesta.IsSuccessStatusCode || (permitirNotFound && respuesta.StatusCode == HttpStatusCode.NotFound))
            return respuesta;

        var mensaje = await LeerMensajeDeErrorAsync(respuesta);
        var estado = respuesta.StatusCode;
        respuesta.Dispose();
        throw new ApiException(mensaje, estado);
    }

    private static async Task<T> LeerCuerpoAsync<T>(HttpResponseMessage respuesta)
    {
        try
        {
         
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await respuesta.Content.ReadFromJsonAsync<T>(options)
                ?? throw new ApiException("La respuesta del servidor está vacía.", respuesta.StatusCode);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new ApiException("Error al procesar la respuesta del servidor.", respuesta.StatusCode, ex);
        }
    }

    protected async Task<T> PostAsync<T>(string url, object cuerpo)
    {
        using var respuesta = await EnviarAsync(() => _http.PostAsJsonAsync(url, cuerpo));
        return await LeerCuerpoAsync<T>(respuesta);
    }

    private static async Task<string> LeerMensajeDeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var json = await respuesta.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

      
            if (root.TryGetProperty("error", out var errorsElement))
            {
                var listaErrores = new List<string>();
                foreach (var prop in errorsElement.EnumerateObject())
                {
                    foreach (var error in prop.Value.EnumerateArray())
                    {
                        listaErrores.Add($"{prop.Name}: {error.GetString()}");
                    }
                }
                if (listaErrores.Count > 0)
                    return "Faltan datos:\n" + string.Join("\n", listaErrores);
            }

            if (root.TryGetProperty("detail", out var detailElement))
                return detailElement.GetString() ?? "Error de validación.";

            if (root.TryGetProperty("title", out var titleElement))
                return titleElement.GetString() ?? "Error de validación.";
        }
        catch { }
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            return "Tu sesión no es válida o ha caducado. Inicia sesión de nuevo.";

        return $"Error del servidor ({(int)respuesta.StatusCode}).";
    }

    private sealed record Problema(string? Title, string? Detail);
}