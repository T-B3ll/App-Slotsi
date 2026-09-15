using Microsoft.Extensions.Logging;
using slotsi_citas.Pages;
using slotsi_citas.ViewModel;
using Syncfusion.Maui.Toolkit.Hosting;
using Supabase;
using slotsi_citas.Services;

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


                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialIcons");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        
        var supabaseUrl = "https://vnasklmkkamytgwymzih.supabase.co";
        var supabaseKey = "sb_publishable_z8qlsYsGV7mYU4iJGGu8NA_fljnaFtq";

        var options = new SupabaseOptions();
        var client = new Client(supabaseUrl, supabaseKey, options);

       

        client.InitializeAsync().Wait();

        // 2. Registrar Servicios Nuevos
        builder.Services.AddSingleton(client);        
        builder.Services.AddSingleton<AuthService>();  

       
        builder.Services.AddTransient<Cita_UsuarioViewModel>();
        builder.Services.AddTransient<LoginViewModel>(); 

        builder.Services.AddTransient<Cita_ClientePage>();
        builder.Services.AddTransient<LoginPage>();      


        // Registrar ViewModels
        builder.Services.AddTransient<Cita_UsuarioViewModel>();

        // Registrar Páginas
        builder.Services.AddTransient<Cita_ClientePage>();




        return builder.Build();
    }
}