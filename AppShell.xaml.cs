using Microsoft.Maui.Controls;
using slotsi_citas.Pages;

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        private bool _hasNavigated = false;

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(NuevoTrabajador), typeof(NuevoTrabajador));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!_hasNavigated)
            {
                _hasNavigated = true;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    var usuarioId = Preferences.Get("UsuarioId", string.Empty);

                    if (string.IsNullOrEmpty(usuarioId))
                    {
                        this.FlyoutBehavior = FlyoutBehavior.Disabled;
                        await Current.GoToAsync("//LoginPage");
                    }
                    else
                    {
                        this.FlyoutBehavior = FlyoutBehavior.Flyout;
                        var esDueno = Preferences.Get("EsDuenoNegocio", false);
                        string ruta = esDueno ? "//Cita_UsuarioPage" : "//Cita_ClientePage";
                        await Current.GoToAsync(ruta);
                    }
                });
            }
        }
    }
}