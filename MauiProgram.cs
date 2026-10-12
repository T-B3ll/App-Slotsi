using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
<<<<<<< HEAD
using slotsi_citas.ViewModels;
=======
using slotsi_citas.ViewModel;
>>>>>>> catalogos
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
<<<<<<< HEAD

             
=======
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialIcons");
>>>>>>> catalogos
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ============================================
        // APPSHELL - REGISTRO
        // ============================================
        builder.Services.AddSingleton<AppShell>();

<<<<<<< HEAD

=======
        // ============================================
        // SUPABASE (SIN BLOQUEAR)
        // ============================================
>>>>>>> catalogos
        var supabaseUrl = "https://vnasklmkkamytgwymzih.supabase.co";
        var supabaseKey = "sb_publishable_z8qlsYsGV7mYU4iJGGu8NA_fljnaFtq";

        var options = new SupabaseOptions();
        var client = new Client(supabaseUrl, supabaseKey, options);

<<<<<<< HEAD


        client.InitializeAsync().Wait();

=======
        // NO usar .Wait() — dejamos que se inicialice en segundo plano
        _ = client.InitializeAsync();

>>>>>>> catalogos
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
<<<<<<< HEAD
        builder.Services.AddTransient<ListaTrabjadoresViewModel>();

        builder.Services.AddTransient<NuevoTrabajadorViewModel>();
        builder.Services.AddTransient<NuevoTrabajador>();
=======
        builder.Services.AddTransient<RegistroUsuariosViewModel>();
>>>>>>> catalogos

        builder.Services.AddTransient<SeleccionarCitaViewModel>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();

        // ============================================
        // PAGES
        // ============================================

        builder.Services.AddTransient<Cita_UsuarioPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegistroUsuarioBasico>();
<<<<<<< HEAD

        builder.Services.AddTransient<Cita_UsuarioViewModel>();

        builder.Services.AddTransient<Cita_ClientePage>();

        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddTransient<RecuperarContra>();
        builder.Services.AddTransient<NuevaContra>();

        builder.Services.AddTransient<NuevoTrabajador>();
        builder.Services.AddTransient<ListaTrabajadores>();

        builder.Services.AddTransient<CalendarioPage>();
        builder.Services.AddTransient<catalogoservicios>();
        builder.Services.AddTransient<DueñosNegociosPage>();
        builder.Services.AddTransient<PagoSuscripcionPage>();
        builder.Services.AddTransient<EditarPerfilPage>();
        builder.Services.AddTransient<EditarPerfilViewModel>();
        builder.Services.AddSingleton<NegocioService>();
        builder.Services.AddTransient<CatalogoCliente>();
        builder.Services.AddTransient<LoginViewModel>();

        builder.Services.AddTransient<PagoSuscripcionViewModel>();
        builder.Services.AddSingleton<SuscripcionService>();

        builder.Services.AddSingleton<EmailJSService>();
        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();
=======
        builder.Services.AddTransient<RegistroUsuariosPage>();
        builder.Services.AddTransient<RecuperarContra>();
        builder.Services.AddTransient<NuevaContra>();
        builder.Services.AddTransient<CalendarioPage>();
        builder.Services.AddTransient<catalogoservicios>();
        builder.Services.AddTransient<CatalogoCliente>();
        builder.Services.AddTransient<SeleccionarCitaPage>();

>>>>>>> catalogos
        return builder.Build();
    }
}