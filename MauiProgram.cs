using Microsoft.Extensions.Logging;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using slotsi_citas.ViewModel;
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

        // ============================================
        // APPSHELL - REGISTRO
        // ============================================
        builder.Services.AddSingleton<AppShell>();

        // ============================================
        // SUPABASE (SIN BLOQUEAR)
        // ============================================
        var supabaseUrl = "https://vnasklmkkamytgwymzih.supabase.co";
        var supabaseKey = "sb_publishable_z8qlsYsGV7mYU4iJGGu8NA_fljnaFtq";

        var options = new SupabaseOptions();
        var client = new Client(supabaseUrl, supabaseKey, options);

        // NO usar .Wait() — dejamos que se inicialice en segundo plano
        _ = client.InitializeAsync();

        builder.Services.AddSingleton(client);
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<UsuarioService>();
        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddSingleton<EmailJSService>();

        // ============================================
        // VIEWMODELS
        // ============================================
      
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<UsuarioBasicoViewModel>();
        builder.Services.AddTransient<RegistroUsuariosViewModel>();

        builder.Services.AddTransient<SeleccionarCitaViewModel>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();

        // ============================================
        // PAGES
        // ============================================

        builder.Services.AddTransient<Cita_UsuarioPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegistroUsuarioBasico>();
        builder.Services.AddTransient<RegistroUsuariosPage>();
        builder.Services.AddTransient<RecuperarContra>();
        builder.Services.AddTransient<NuevaContra>();
        builder.Services.AddTransient<CalendarioPage>();
        builder.Services.AddTransient<catalogoservicios>();
        builder.Services.AddTransient<CatalogoCliente>();
        builder.Services.AddTransient<SeleccionarCitaPage>();

        return builder.Build();
    }
}