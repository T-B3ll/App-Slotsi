using Microsoft.Extensions.Logging;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.Services.Api;
using slotsi_citas.Services.Auth;
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

        builder.Services.AddTransient<HorariosDisponiblesPage>();
        builder.Services.AddTransient<HorariosDisponiblesViewModel>();
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

        builder.Services.AddSingleton<ISesionService, SesionService>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        // --- SERVICIOS HTTP (API) ---
        // Todos pasan por AuthHeaderHandler y comparten la misma configuración.
        builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(ConfigurarClienteApi)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<UsuarioService>(ConfigurarClienteApi)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<NegocioService>(ConfigurarClienteApi)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<CatalogoService>(ConfigurarClienteApi)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<TrabajadorService>(ConfigurarClienteApi)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<ResumenService>(ConfigurarClienteApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddTransient<ResumenViewModel>();
        builder.Services.AddTransient<Pages.ResumenPage>();
        builder.Services.AddHttpClient<CalificacionService>(ConfigurarClienteApi)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<CitaService>(c =>
          c.BaseAddress = ApiConfiguracion.UrlBase);

        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddSingleton<SuscripcionService>();
        builder.Services.AddSingleton<EmailJSService>();
        builder.Services.AddHttpClient<CitaService>(c => c.BaseAddress = ApiConfiguracion.UrlBase);
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

        builder.Services.AddTransient<HorariosDisponiblesPage>();
        builder.Services.AddTransient<HorariosDisponiblesViewModel>();
        builder.Services.AddTransient<Cita_UsuarioPage>();
        builder.Services.AddTransient<CentroCalificaciones>();
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
        builder.Services.AddTransient<ReportesMenuPage>();
        builder.Services.AddTransient<ReporteDemandaPage>();

        builder.Services.AddTransient<DueñosNegociosViewModel>();
        builder.Services.AddTransient<DueñosNegociosPage>();

        builder.Services.AddTransient<SeleccionarCitaViewModel>();
        builder.Services.AddTransient<SeleccionarCitaPage>();


        return builder.Build();
    }

    private static void ConfigurarClienteApi(HttpClient cliente)
    {
        cliente.BaseAddress = ApiConfiguracion.UrlBase;
        // Si el servidor no responde, falla en 10 segundos en lugar de los 100 por defecto.
        cliente.Timeout = TimeSpan.FromSeconds(10);
    }
}