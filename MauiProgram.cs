using Microsoft.Extensions.Logging;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using slotsi_citas.ViewModels;
using slotsi_citas.Services.Api;
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
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // --- SUPABASE ---
        var supabaseUrl = "https://vnasklmkkamytgwymzih.supabase.co";
        var supabaseKey = "sb_publishable_z8qlsYsGV7mYU4iJGGu8NA_fljnaFtq";
        var options = new SupabaseOptions();
        var client = new Client(supabaseUrl, supabaseKey, options);
        client.InitializeAsync().Wait();

        builder.Services.AddSingleton(client);


        builder.Services.AddSingleton<MongoService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<SupabaseStorageService>();

        // --- SERVICIOS HTTP (API)
        builder.Services.AddHttpClient<UsuarioService>(c =>
            c.BaseAddress = ApiConfiguracion.UrlBase);

        builder.Services.AddHttpClient<NegocioService>(c =>
            c.BaseAddress = ApiConfiguracion.UrlBase);

        builder.Services.AddHttpClient<CatalogoService>(c =>
            c.BaseAddress = ApiConfiguracion.UrlBase);
   

        builder.Services.AddHttpClient<TrabajadorService>(c =>
            c.BaseAddress = ApiConfiguracion.UrlBase);

        builder.Services.AddHttpClient<CalificacionService>(c =>
            c.BaseAddress = ApiConfiguracion.UrlBase);

        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddSingleton<SuscripcionService>();
        builder.Services.AddSingleton<EmailJSService>();

        // --- VIEWMODELS ---
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CitasClienteViewModel>();
        builder.Services.AddTransient<CatalogoServiciosViewModel>();
        builder.Services.AddTransient<UsuarioBasicoViewModel>();
        builder.Services.AddTransient<EditarServicioViewModel>();
        builder.Services.AddTransient<RegistrarNegocioViewModel>();
        builder.Services.AddTransient<ListaTrabjadoresViewModel>();
        builder.Services.AddTransient<NuevoTrabajadorViewModel>();
        builder.Services.AddTransient<PagoSuscripcionViewModel>();
        builder.Services.AddTransient<CrearResenaViewModel>();
        builder.Services.AddTransient<CentroReseñasViewModel>();
        builder.Services.AddTransient<EditarPerfilViewModel>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CrearResena>();
        builder.Services.AddTransient<RegistroUsuarioBasico>();
        builder.Services.AddTransient<RegistroUsuarioNegocio>();
        builder.Services.AddTransient<NuevoTrabajador>();
        builder.Services.AddTransient<ListaTrabajadores>();
        builder.Services.AddTransient<CalendarioPage>();
        builder.Services.AddTransient<catalogoservicios>();
        builder.Services.AddTransient<CatalogoCliente>();
        builder.Services.AddTransient<DueñosNegociosPage>();
        builder.Services.AddTransient<PagoSuscripcionPage>();
        builder.Services.AddTransient<EditarPerfilPage>();
        builder.Services.AddTransient<CatalogoCliente>();
        builder.Services.AddTransient<RecuperarContra>();
        builder.Services.AddTransient<NuevaContra>();


        builder.Services.AddTransient<DueñosNegociosViewModel>();
        builder.Services.AddTransient<DueñosNegociosPage>();


        return builder.Build();
    }
}