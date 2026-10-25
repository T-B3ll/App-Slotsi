using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.Services.Api;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthApiService _auth;
        private readonly UsuarioService _usuarioService;
        private readonly NegocioService _negocioService;
        private string _email;
        private string _password;
        private bool _isBusy;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        // COMANDO DE INICIO DE SESIÓN
        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthApiService auth, UsuarioService usuarioService, NegocioService negocioService)
        {
            _auth = auth;
            _usuarioService = usuarioService;
            _negocioService = negocioService;
            LoginCommand = new Command(async () => await ExecuteLogin());
        }

        private async Task ExecuteLogin()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingresa correo y contraseña", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                await ValidarYEntrarAsync(Email.Trim(), Password);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [LOGIN] Error crítico: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ValidarYEntrarAsync(string email, string password)
        {
            var correo = email.Trim().ToLowerInvariant();

            // 1) Login contra la API: valida credenciales y guarda el token JWT en la sesión.
            try
            {
                await _auth.IniciarSesionAsync(correo, password);
            }
            catch (ApiException ex)
            {
                // 401 con "Correo o Contraseña incorrectas", error de red, etc.
                await Application.Current.MainPage.DisplayAlert("No se pudo iniciar sesión", ex.Message, "OK");
                return;
            }

            // 2) Datos del usuario (la petición ya viaja con el token).
            Usuario? usuario;
            try
            {
                usuario = await _usuarioService.ObtenerPorCorreoAsync(correo);
            }
            catch (ApiException ex)
            {
                await _auth.CerrarSesionAsync();
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                return;
            }

            if (usuario == null)
            {
                await _auth.CerrarSesionAsync();
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el usuario.", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ [LOGIN] Entró: {usuario.NombreCompleto} (Tipo: {usuario.TipoUsuario})");

            Password = string.Empty;

            string idUsuarioString = usuario.Id?.ToString() ?? "";
            Preferences.Set("UsuarioId", idUsuarioString);
            Preferences.Set("UsuarioIdSesion", idUsuarioString);
            Preferences.Set("EsDuenoNegocio", usuario.TipoUsuario);
            Preferences.Set("UsuarioCorreo", usuario.Correo);

            Preferences.Remove("negocio_id");
            if (usuario.TipoUsuario)
            {
                try
                {
                    var negocio = await _negocioService.ObtenerPorUsuarioIdAsync(idUsuarioString);
                    if (!string.IsNullOrEmpty(negocio?.Id))
                        Preferences.Set("negocio_id", negocio.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [LOGIN] No se pudo obtener el negocio: {ex.Message}");
                }
            }

            Application.Current.MainPage = new AppShell(_auth);

            string ruta = usuario.TipoUsuario ? "//catalogoservicios" : "//dueñosnegocios";

            System.Diagnostics.Debug.WriteLine($" [LOGIN] Navegando a: {ruta}");

            try
            {
                await Shell.Current.GoToAsync(ruta);
            }
            catch (Exception navEx)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [LOGIN] Error navegando: {navEx.Message}");
                await Shell.Current.GoToAsync("//Inicio");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}