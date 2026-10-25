using Slotsi.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace slotsi_citas.Services.Auth
{
    public class SesionService : ISesionService
    {
        private const string ClaveToken = "jwt_token";
        private const string ClaveExpira = "jwt_expira";
        private string? _token;
        private DateTime _expiraEn;
        public string? Token => EstaAutenticado ? _token : null;

        public bool EstaAutenticado => !string.IsNullOrEmpty(_token) && _expiraEn > DateTime.UtcNow;
        public async Task<bool> RestaurarAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync(ClaveToken);
                var expira = await SecureStorage.Default.GetAsync(ClaveExpira);
                if (token is not null && DateTime.TryParse(expira, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var fecha)
                && fecha > DateTime.UtcNow)
                {
                    _token = token;
                    _expiraEn = fecha;
                    PublicarNombre(token);
                    return true;
                }
            }
            catch (Exception)
            {
                // SecureStorage no disponible (p. ej. Windows sin empaquetar): no hay sesión guardada.
            }
            Cerrar();
            return false;
        }
        public async Task IniciarAsync(TokenDto token)
        {
            _token = token.Token;
            _expiraEn = token.ExpiraEn.ToUniversalTime();
            PublicarNombre(token.Token);
            try
            {
                await SecureStorage.Default.SetAsync(ClaveToken, token.Token);
                await SecureStorage.Default.SetAsync(ClaveExpira, _expiraEn.ToString("O",
               CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                // Sin almacenamiento seguro la sesión vive solo en memoria: hay que
                // iniciar sesión de nuevo al reabrir la app.
            }
        }
        public void Cerrar()
        {
            _token = null;
            _expiraEn = default;
            try
            {
                SecureStorage.Default.Remove(ClaveToken);
                SecureStorage.Default.Remove(ClaveExpira);
            }
            catch (Exception)
            {
                // Nada que borrar si el almacenamiento no está disponible.
            }
        }
        // El saludo de TareasPage lee "nombre_usuario": se toma del claim "name" del JWT.
        private static void PublicarNombre(string jwt)
        {
            try
            {
                var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
                if (doc.RootElement.TryGetProperty("name", out var nombre) && nombre.GetString() is { } valor)
                    Preferences.Default.Set("nombre_usuario", valor);
            }
            catch (Exception)
            {
                // Token con formato inesperado: el saludo usará el valor por defecto.
            }

        }
    }
}
