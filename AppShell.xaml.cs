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
            Routing.RegisterRoute(nameof(CrearResena), typeof(CrearResena));
            Routing.RegisterRoute(nameof(RegistroUsuariosPage), typeof(RegistroUsuariosPage));
            Routing.RegisterRoute(nameof(RegistroUsuarioNegocio), typeof(RegistroUsuarioNegocio));
            Routing.RegisterRoute(nameof(CatalogoCliente), typeof(CatalogoCliente));
            Routing.RegisterRoute(nameof(CalendarioPage), typeof(CalendarioPage));
            Routing.RegisterRoute(nameof(HorariosDisponiblesPage), typeof(HorariosDisponiblesPage));
            Routing.RegisterRoute(nameof(ReportesMenuPage), typeof(ReportesMenuPage));
            Routing.RegisterRoute(nameof(ReporteTopServiciosPage), typeof(ReporteTopServiciosPage));
            Routing.RegisterRoute(nameof(ReporteDemandaPage), typeof(ReporteDemandaPage));
            Routing.RegisterRoute(nameof(CancelacionesPage), typeof(CancelacionesPage));
        }
        private async void OnCerrarSesionClicked(object? sender, EventArgs e)
        {
            botonCerrarSesion.IsEnabled = false;
            FlyoutIsPresented = false;
            try
            {
                await _auth.CerrarSesionAsync();
            }
            finally
            {
                NavigationAuth.IrALogin();
            }
        }
    }
}