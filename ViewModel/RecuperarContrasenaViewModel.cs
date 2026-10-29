using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using slotsi_citas.Services;
using Microsoft.Maui.Controls;

namespace slotsi_citas.ViewModel
{
    public class RecuperarContrasenaViewModel : INotifyPropertyChanged
    {
        private readonly RecuperacionService _recuperacionService;
        private readonly EmailJSService _emailService;
        private string _correo = string.Empty;

        public string Correo
        {
            get => _correo;
            set { _correo = value; OnPropertyChanged(); }
        }

        public ICommand EnviarCommand { get; }

        public RecuperarContrasenaViewModel(RecuperacionService recuperacionService, EmailJSService emailService)
        {
            _recuperacionService = recuperacionService;
            _emailService = emailService;
            EnviarCommand = new Command(async () => await EnviarCodigoAsync());
        }

        private async Task EnviarCodigoAsync()
        {
            if (string.IsNullOrWhiteSpace(Correo))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingresa un correo.", "OK");
                return;
            }

            try
            {
              
                string codigoReal = await _recuperacionService.GenerarSolicitudAsync(Correo);

                System.Diagnostics.Debug.WriteLine($"[RECUPERACION] Código generado: {codigoReal} para {Correo}");

          
                bool enviado = await _emailService.EnviarCodigoPorEmail(Correo, codigoReal);

                if (enviado)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito",
                        $"Código enviado a {Correo}. Revisa tu bandeja de entrada o spam.", "OK");

                  
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error de envío",
                        "No se pudo enviar el correo. Intenta de nuevo más tarde.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR CRÍTICO EN VIEWMODEL: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error Crítico", ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}