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
                
                string codigoPrueba = "123456";

                System.Diagnostics.Debug.WriteLine($" INICIANDO ENVÍO A: {Correo} CON CÓDIGO: {codigoPrueba}");

               
                bool enviado = await _emailService.EnviarCodigoPorEmail(Correo, codigoPrueba);

                System.Diagnostics.Debug.WriteLine($"📩 RESULTADO DEL ENVÍO: {enviado}");

                if (enviado)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito",
                        $"Código enviado a {Correo}. Revisa spam.", "OK");
                }
                else
                {
                    
                    await Application.Current.MainPage.DisplayAlert("Fallo el envío",
                        $"El correo no salió, pero usa este código manual: {codigoPrueba}", "OK");
                }
            }
            catch (Exception ex)
            {
               
                System.Diagnostics.Debug.WriteLine($"💥 ERROR CRÍTICO EN VIEWMODEL: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"💥 DETALLE: {ex.StackTrace}");

                await Application.Current.MainPage.DisplayAlert("Error Crítico", ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}