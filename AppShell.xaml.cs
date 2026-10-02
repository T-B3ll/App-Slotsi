using slotsi_citas.Pages;

namespace slotsi_citas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(NuevoTrabajador), typeof(NuevoTrabajador));
        }
    }
}
