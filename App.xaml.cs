using Microsoft.Extensions.DependencyInjection;

namespace slotsi_citas
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // ❌ ELIMINADO: MainPage = new NavigationPage(...) o MainPage = new AppShell();
        }

        // ✅ MÉTODO CORRECTO PARA .NET MAUI MODERNO
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}