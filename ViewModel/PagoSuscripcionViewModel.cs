using Microsoft.Maui.Controls;
using slotsi_citas.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class PagoSuscripcionViewModel : INotifyPropertyChanged
    {
        private readonly SuscripcionService _suscripcionService;

        private string _numeroTarjeta = string.Empty;
        private string _fechaExpiracion = string.Empty;
        private string _cvv = string.Empty;
        private string _nombreTitular = string.Empty;
        private string _metodoSeleccionado = "Débito";

        private bool _estaProcesando = false;
        private string _textoBoton = "Pagar $5 USD";

        public event PropertyChangedEventHandler PropertyChanged;


        public string NumeroTarjeta
        {
            get => _numeroTarjeta;
            set { _numeroTarjeta = value; OnPropertyChanged(); }
        }

        public string FechaExpiracion
        {
            get => _fechaExpiracion;
            set { _fechaExpiracion = value; OnPropertyChanged(); }
        }

        public string Cvv
        {
            get => _cvv;
            set { _cvv = value; OnPropertyChanged(); }
        }

        public string NombreTitular
        {
            get => _nombreTitular;
            set { _nombreTitular = value; OnPropertyChanged(); }
        }

        public string MetodoSeleccionado
        {
            get => _metodoSeleccionado;
            set { _metodoSeleccionado = value; OnPropertyChanged(); }
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            set { _estaProcesando = value; OnPropertyChanged(); }
        }

        public string TextoBoton
        {
            get => _textoBoton;
            set { _textoBoton = value; OnPropertyChanged(); }
        }

        public ICommand PagarCommand { get; }

        public PagoSuscripcionViewModel(SuscripcionService suscripcionService)
        {
            _suscripcionService = suscripcionService;
            PagarCommand = new Command(async () => await EjecutarPago(), () => !EstaProcesando);
        }

        private async Task EjecutarPago()
        {
       
            if (string.IsNullOrWhiteSpace(NumeroTarjeta) ||
                string.IsNullOrWhiteSpace(FechaExpiracion) ||
                string.IsNullOrWhiteSpace(Cvv) ||
                string.IsNullOrWhiteSpace(NombreTitular))
            {
                await Application.Current.MainPage.DisplayAlert("Campos incompletos", "Por favor llena todos los datos.", "OK");
                return;
            }

            // Validación de formato tarjeta (16 dígitos limpios)
            if (NumeroTarjeta.Length != 16)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El número de tarjeta debe tener 16 dígitos.", "OK");
                return;
            }

        
            var partesFecha = FechaExpiracion.Split('/');
            if (partesFecha.Length != 2 || partesFecha[0].Length != 2 || partesFecha[1].Length != 2)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Formato de fecha inválido. Usa MM/AA.", "OK");
                return;
            }

            int mes = int.Parse(partesFecha[0]);
            if (mes < 1 || mes > 12)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Mes inválido (01-12).", "OK");
                return;
            }

         
            if (Cvv.Length < 3 || Cvv.Length > 4)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "CVV debe tener 3 o 4 dígitos.", "OK");
                return;
            }

            EstaProcesando = true;
            TextoBoton = "Procesando...";
            ((Command)PagarCommand).ChangeCanExecute();

            try
            {
                await Task.Delay(1500); // Simulación

                var usuarioId = Preferences.Get("UsuarioId", string.Empty);
                if (string.IsNullOrEmpty(usuarioId))
                    throw new Exception("Sesión no activa.");

                // Procesar pago en MongoDB
                await _suscripcionService.ProcesarPagoExitosoAsync(
                    usuarioId,
                    monto: 5.00m,
                    metodoPago: MetodoSeleccionado);

                await Application.Current.MainPage.DisplayAlert(
                    "¡Pago Exitoso!",
                    $"Suscripción activa hasta {DateTime.UtcNow.AddMonths(1):dd MMM, yyyy}.",
                    "Genial");

                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                EstaProcesando = false;
                TextoBoton = "Pagar $5 USD";
                ((Command)PagarCommand).ChangeCanExecute();
            }
        }

        private void LimpiarFormulario()
        {
            NumeroTarjeta = string.Empty;
            FechaExpiracion = string.Empty;
            Cvv = string.Empty;
            NombreTitular = string.Empty;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}