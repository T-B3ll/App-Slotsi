using Microsoft.Maui.Storage;
using slotsi_citas.Pages;
using slotsi_citas.Services.Api;

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        private readonly IAuthApiService _auth;
        public AppShell(IAuthApiService auth)
        {
            InitializeComponent();
            _auth = auth;
            Routing.RegisterRoute(nameof(NuevoTrabajador), typeof(NuevoTrabajador));
            Routing.RegisterRoute(nameof(CatalogoCliente), typeof(CatalogoCliente));
            Routing.RegisterRoute(nameof(DueñosNegociosPage), typeof(DueñosNegociosPage));
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
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
                botonCerrarSesion.IsEnabled = true;
            }
        }
    }
}