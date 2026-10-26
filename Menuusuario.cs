using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas
{
    public static class MenuUsuario
    {
        public static bool EsDueno => Preferences.Get("EsDuenoNegocio", false);
        public static bool EsCliente => !EsDueno;
    }
}
