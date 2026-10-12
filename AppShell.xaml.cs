using slotsi_citas.Pages;
<<<<<<< HEAD
using Microsoft.Maui.Storage;
=======
>>>>>>> catalogos

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
<<<<<<< HEAD
            Routing.RegisterRoute(nameof(NuevoTrabajador), typeof(NuevoTrabajador));
            Routing.RegisterRoute(nameof(DueñosNegociosPage), typeof(DueñosNegociosPage));
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Cerrar Sesión", "¿Deseas salir de la cuenta?", "Sí", "Cancelar");
            if (!confirmar) return;

            Preferences.Remove("UsuarioId");
            Preferences.Remove("UsuarioIdSesion");
            Preferences.Remove("EsDuenoNegocio");
            Preferences.Remove("UsuarioCorreo");

            var serviceProvider = Handler?.MauiContext?.Services;
            if (serviceProvider != null)
            {
                var loginPage = serviceProvider.GetRequiredService<Pages.LoginPage>();
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
=======

            // Rutas internas para la navegación GoToAsync
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
        
            Routing.RegisterRoute(nameof(Cita_UsuarioPage), typeof(Cita_UsuarioPage));

            // Rutas para el flujo de login y registro
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegistroUsuariosPage), typeof(RegistroUsuariosPage));
            Routing.RegisterRoute(nameof(RegistroUsuarioNegocio), typeof(RegistroUsuarioNegocio));

            // Rutas para catálogos y calendario
            Routing.RegisterRoute(nameof(catalogoservicios), typeof(catalogoservicios));
            Routing.RegisterRoute(nameof(CatalogoCliente), typeof(CatalogoCliente));
            Routing.RegisterRoute(nameof(CalendarioPage), typeof(CalendarioPage));
>>>>>>> catalogos
        }
    }
}