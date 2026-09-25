using Microsoft.Extensions.DependencyInjection;
using slotsi_citas.Pages;

namespace slotsi_citas
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var citaUsuarioPage = _serviceProvider.GetRequiredService<Cita_UsuarioPage>();
            return new Window(new NavigationPage(citaUsuarioPage));
        }
    }
}