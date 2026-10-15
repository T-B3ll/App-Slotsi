using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace slotsi_citas.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly UsuarioService _usuarioService;
        private string _email;
        private string _password;
        private bool _isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public ICommand LoginCommand { get; }

        public LoginViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            LoginCommand = new Command(async () => await ExecuteLogin());
        }

        private async Task ExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingresa correo y contraseña", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                await ValidarYEntrarAsync(Email, Password);
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

        private string EncriptarPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private async Task ValidarYEntrarAsync(string identificador, string password)
        {
            Usuario usuario = null;
            var textoLimpio = identificador?.Trim().ToLower() ?? string.Empty;

            System.Diagnostics.Debug.WriteLine($"🔍 [LOGIN] Buscando: '{textoLimpio}'");

            try
            {
       
                usuario = await _usuarioService.ObtenerPorCorreoAsync(textoLimpio);

               
                if (usuario == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [LOGIN] No por correo. Buscando por nombre...");
                    usuario = await _usuarioService.ObtenerPorIdentificadorAsync(identificador);
                }
            }
            catch (Exception apiEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [LOGIN] Error API: {apiEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (usuario == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [LOGIN] Usuario NO encontrado.");
                await Application.Current.MainPage.DisplayAlert("Error", "Usuario/Correo o contraseña incorrectos", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ [LOGIN] Encontrado: {usuario.NombreCompleto} (Tipo: {usuario.TipoUsuario})");

           
            string passwordEncriptada = EncriptarPassword(password);
            System.Diagnostics.Debug.WriteLine($"🔐 [LOGIN] Hash App: '{passwordEncriptada}'");
            System.Diagnostics.Debug.WriteLine($"💾 [LOGIN] Hash BD:   '{usuario.Contrasena}'");

            bool coinciden = !string.IsNullOrEmpty(usuario.Contrasena) &&
                             usuario.Contrasena == passwordEncriptada;

            if (!coinciden)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [LOGIN] Contraseña INCORRECTA.");
                await Application.Current.MainPage.DisplayAlert("Error", "Usuario/Correo o contraseña incorrectos", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"🎉 [LOGIN] ¡ÉXITO! Guardando sesión...");

           
            string idUsuarioString = usuario.Id?.ToString() ?? "";
            Preferences.Set("UsuarioId", idUsuarioString);
            Preferences.Set("UsuarioIdSesion", idUsuarioString);
            Preferences.Set("EsDuenoNegocio", usuario.TipoUsuario);
            Preferences.Set("UsuarioCorreo", usuario.Correo);

            
            Application.Current.MainPage = new AppShell();
            await Task.Delay(100); 

            
            string ruta = usuario.TipoUsuario ? "//ListaTrabajadores" : "//dueñosnegocios";

            System.Diagnostics.Debug.WriteLine($"🧭 [LOGIN] Navegando a: {ruta}");

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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}