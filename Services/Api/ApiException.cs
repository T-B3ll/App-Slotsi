using System.Net;

namespace slotsi_citas.Services.Api;

public class ApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public ApiException(string mensaje, HttpStatusCode? statusCode = null, Exception? inner = null)
        : base(mensaje, inner)
    {
        StatusCode = statusCode;
    }

    public static string MensajePara(Exception ex) =>
        ex is ApiException apiEx ? apiEx.Message : $"Error inesperado: {ex.Message}";
}