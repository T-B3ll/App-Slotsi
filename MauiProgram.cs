using Microsoft.Extensions.Logging;
using slotsi_citas.Pages;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using slotsi_citas.ViewModels;
using Supabase;
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



                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialIcons");

                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");

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
        builder.Services.AddSingleton<UsuarioService>();


        builder.Services.AddTransient<Cita_UsuarioViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<UsuarioBasicoViewModel>();

        builder.Services.AddTransient<Cita_ClientePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegistroUsuarioBasico>();



        builder.Services.AddTransient<Cita_UsuarioViewModel>();
        builder.Services.AddTransient<Cita_ClientePage>();

        builder.Services.AddTransient<Cita_UsuarioViewModel>();
        builder.Services.AddTransient<Cita_UsuarioPage>();

        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddTransient<RecuperarContra>();
        builder.Services.AddTransient<NuevaContra>();

        builder.Services.AddTransient<CalendarioPage>();
        builder.Services.AddTransient<catalogoservicios>();

        builder.Services.AddTransient<Cita_ClientePage>();
        builder.Services.AddTransient<Cita_clienteViewModel>();

        builder.Services.AddTransient<SeleccionarCitaPage>();
        builder.Services.AddTransient<SeleccionarCitaViewModel>();

        builder.Services.AddSingleton<EmailJSService>();
        builder.Services.AddSingleton<RecuperacionService>(); 
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();
        return builder.Build();
    }
}