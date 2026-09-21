using System.Net.Http.Json;
using System.Text.Json;

namespace slotsi_citas.Services
{
    public class EmailJSService
    {
     
        private const string ServiceId = "service_ha3iwlg";
        private const string TemplateId = "template_wp83z2q";
        private const string PublicKey = "6TNTvUL6bWpa1mWkH";

        public async Task<bool> EnviarCodigoPorEmail(string correoDestino, string codigo)
        {
            try
            {
              
                var payload = new
                {
                    service_id = ServiceId,
                    template_id = TemplateId,
                    user_id = PublicKey, 
                    template_params = new
                    {
                        to_email = correoDestino,
                        codigo = codigo
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await new HttpClient().PostAsync(
                    "https://api.emailjs.com/api/v1.0/email/send",
                    content
                );

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("✅ EMAILJS: Correo enviado exitosamente");
                    return true;
                }
                else
                {
                    
                    System.Diagnostics.Debug.WriteLine($"❌ EMAILJS ERROR [{response.StatusCode}]: {responseBody}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 EMAILJS EXCEPTION: {ex.Message}");
                return false;
            }
        }
    }
}