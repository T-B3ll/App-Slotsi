using Microsoft.Extensions.Logging;
using slotsi_citas.Pages;
using slotsi_citas.ViewModel;
using Syncfusion.Maui.Toolkit.Hosting;

namespace slotsi_citas;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Registrar ViewModels
        builder.Services.AddTransient<Cita_UsuarioViewModel>();

        // Registrar Páginas
        builder.Services.AddTransient<Cita_ClientePage>();

        return builder.Build();
    }
}