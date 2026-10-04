using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using Microsoft.Extensions.DependencyInjection;
using slotsi_citas.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace slotsi_citas.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly UsuarioService _usuarioService;
        private readonly IServiceProvider _serviceProvider;
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

        public LoginViewModel(UsuarioService usuarioService, IServiceProvider serviceProvider)
        {
            _usuarioService = usuarioService;
            _serviceProvider = serviceProvider;
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
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ValidarYEntrarAsync(string identificador, string password)
        {
            var usuario = await _usuarioService.ObtenerPorIdentificadorAsync(identificador);

            if (usuario == null || usuario.Contrasena != password)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Usuario/Correo o contraseña incorrectos", "OK");
                return;
            }

            string idUsuarioString = usuario.Id.ToString();

            Preferences.Set("UsuarioId", idUsuarioString);
            Preferences.Set("UsuarioIdSesion", idUsuarioString); 
            Preferences.Set("EsDuenoNegocio", usuario.TipoUsuario);
            Preferences.Set("UsuarioCorreo", usuario.Correo);

            Application.Current.MainPage = new AppShell();

            if (!usuario.TipoUsuario)
            {
                await Shell.Current.GoToAsync("//catalogocliente");
            }
            else
            {
                await Shell.Current.GoToAsync("//ListaTrabajadores");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}