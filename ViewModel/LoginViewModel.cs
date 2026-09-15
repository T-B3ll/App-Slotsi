using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using slotsi_citas.Services;


namespace slotsi_citas.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
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

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
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
            
                var user = await _authService.LoginAsync(Email, Password);

                if (user != null)
                {
                   
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Credenciales incorrectas", "OK");
                }
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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
