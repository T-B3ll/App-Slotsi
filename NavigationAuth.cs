using slotsi_citas.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas
{
    public static class NavigationAuth
    {
        public static void IrALogin()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Preferences.Remove("UsuarioId");
                Preferences.Remove("UsuarioIdSesion");
                Preferences.Remove("EsDuenoNegocio");
                Preferences.Remove("UsuarioCorreo");
                Preferences.Remove("negocio_id");
                Preferences.Remove("NegocioSeleccionadoId");
                Preferences.Remove("ServiciosSeleccionadosIds");

                var login = IPlatformApplication.Current!.Services.GetRequiredService<LoginPage>();
                var raiz = new NavigationPage(login);

                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = raiz;
            });
        }
    }
}
