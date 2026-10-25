using slotsi_citas.Pages;
using Microsoft.Maui.Storage;

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(NuevoTrabajador), typeof(NuevoTrabajador));
            Routing.RegisterRoute(nameof(CatalogoCliente), typeof(CatalogoCliente));
            Routing.RegisterRoute(nameof(DueñosNegociosPage), typeof(DueñosNegociosPage));
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
            Routing.RegisterRoute(nameof(Cita_ClientePage), typeof(Cita_ClientePage));
            Routing.RegisterRoute(nameof(Cita_UsuarioPage), typeof(Cita_UsuarioPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(CrearResena), typeof(CrearResena));
            Routing.RegisterRoute(nameof(RegistroUsuariosPage), typeof(RegistroUsuariosPage));
            Routing.RegisterRoute(nameof(RegistroUsuarioNegocio), typeof(RegistroUsuarioNegocio));
            Routing.RegisterRoute(nameof(CatalogoCliente), typeof(CatalogoCliente));
            Routing.RegisterRoute(nameof(CalendarioPage), typeof(CalendarioPage));

        }
        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            botonCerrarSesion.IsEnabled = false;
            try
            {
                await _auth.CerrarSesionAsync(); // revoca el token y borra la sesión local[cite: 18]
                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
            finally
            {
                var loginPage = serviceProvider.GetRequiredService<Pages.LoginPage>();
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
        }
    }
}