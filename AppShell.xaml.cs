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
        }

            // Rutas internas para la navegación GoToAsync
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
            Routing.RegisterRoute(nameof(Cita_ClientePage), typeof(Cita_ClientePage));
            Routing.RegisterRoute(nameof(Cita_UsuarioPage), typeof(Cita_UsuarioPage));
        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Cerrar Sesión", "¿Deseas salir de la cuenta?", "Sí", "Cancelar");
            if (!confirmar) return;

            Preferences.Remove("UsuarioId");
            Preferences.Remove("UsuarioIdSesion");
            Preferences.Remove("EsDuenoNegocio");
            Preferences.Remove("UsuarioCorreo");

            // Rutas para el flujo de login y registro
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegistroUsuariosPage), typeof(RegistroUsuariosPage));
            Routing.RegisterRoute(nameof(RegistroUsuarioNegocio), typeof(RegistroUsuarioNegocio));

            // Rutas para catálogos y calendario
            Routing.RegisterRoute(nameof(catalogoservicios), typeof(catalogoservicios));
            Routing.RegisterRoute(nameof(CatalogoCliente), typeof(CatalogoCliente));
            Routing.RegisterRoute(nameof(CalendarioPage), typeof(CalendarioPage));
            var serviceProvider = Handler?.MauiContext?.Services;
            if (serviceProvider != null)
            {
                var loginPage = serviceProvider.GetRequiredService<Pages.LoginPage>();
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
        }
    }
}