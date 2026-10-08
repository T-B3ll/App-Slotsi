using slotsi_citas.Pages;
using Microsoft.Maui.Storage;

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas que no están en el XAML pero se usan por código
            Routing.RegisterRoute(nameof(NuevoTrabajador), typeof(NuevoTrabajador));
            Routing.RegisterRoute(nameof(DueñosNegociosPage), typeof(DueñosNegociosPage));
            Routing.RegisterRoute(nameof(SeleccionarCitaPage), typeof(SeleccionarCitaPage));
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Cerrar Sesión", "¿Deseas salir de la cuenta?", "Sí", "Cancelar");
            if (!confirmar) return;

            // Limpiar datos sensibles
            Preferences.Remove("UsuarioId");
            Preferences.Remove("UsuarioIdSesion");
            Preferences.Remove("EsDuenoNegocio");
            Preferences.Remove("UsuarioCorreo");

            // ✅ CORRECTO: Navegar por ruta, NO cambiar MainPage
            // Las "//" aseguran que limpie la pila de navegación y vaya al login limpio
            await GoToAsync("//LoginPage");
        }
    }
}