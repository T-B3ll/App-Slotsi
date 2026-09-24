using slotsi_citas.Pages;

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registro de rutas internas para la navegación GoToAsync
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
            Routing.RegisterRoute(nameof(Cita_ClientePage), typeof(Cita_ClientePage));
        }
    }
}