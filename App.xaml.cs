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
            var loginPage = serviceProvider.GetRequiredService<Pages.LoginPage>();
            MainPage = new NavigationPage(loginPage);
        }

    }
}