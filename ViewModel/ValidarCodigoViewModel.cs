using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using slotsi_citas.Models;
using slotsi_citas.Services;
using Microsoft.Maui.Controls;

namespace slotsi_citas.ViewModel
{
    public class ValidarCodigoViewModel : INotifyPropertyChanged
    {
        private readonly RecuperacionService _recuperacionService;

    
        public string CorreoDestino { get; set; } = string.Empty;

        private string _codigo = string.Empty;
        public string Codigo
        {
            get => _codigo;
            set { _codigo = value; OnPropertyChanged(); }
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        public ICommand ValidarCommand { get; }

        public ValidarCodigoViewModel(RecuperacionService recuperacionService)
        {
            _recuperacionService = recuperacionService;

            ValidarCommand = new Command(async () => await ValidarAsync(),
                                         () => !EstaCargando && !string.IsNullOrWhiteSpace(Codigo));
        }

        private async Task ValidarAsync()
        {
            if (string.IsNullOrWhiteSpace(Codigo) || Codigo.Length != 6)
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "El código debe tener 6 dígitos.", "OK");
                return;
            }

            EstaCargando = true;
            ((Command)ValidarCommand).ChangeCanExecute();

            try
            {
                var solicitud = await _recuperacionService.ValidarCodigoAsync(CorreoDestino, Codigo);

                if (solicitud != null)
                {
                   
                    await Shell.Current.GoToAsync($"//NuevaContrasena?solicitudId={solicitud.Id}&correo={Uri.EscapeDataString(CorreoDestino)}");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Código inválido o expirado. Intenta de nuevo.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                EstaCargando = false;
                ((Command)ValidarCommand).ChangeCanExecute();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (name == nameof(Codigo)) ((Command)ValidarCommand).ChangeCanExecute();
        }
    }
}