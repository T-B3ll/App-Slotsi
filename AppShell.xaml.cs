using slotsi_citas.Pages;

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

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
        }
    }
}