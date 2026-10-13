using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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

        builder.Services.AddSingleton<AppShell>();


        var supabaseUrl = "https://vnasklmkkamytgwymzih.supabase.co";
        var supabaseKey = "sb_publishable_z8qlsYsGV7mYU4iJGGu8NA_fljnaFtq";

        var options = new SupabaseOptions();
        var client = new Client(supabaseUrl, supabaseKey, options);

        _ = client.InitializeAsync();

        builder.Services.AddSingleton(client);
        Task.Run(async () => await client.InitializeAsync());

        builder.Services.AddSingleton(client);

        builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(MongoDbSettings.ConnectionString));

        builder.Services.AddSingleton<IMongoDatabase>(sp =>
        {
            var mongoClient = sp.GetRequiredService<IMongoClient>();
            return mongoClient.GetDatabase(MongoDbSettings.DatabaseName);
        });

        builder.Services.AddSingleton<MongoService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<UsuarioService>();
        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddSingleton<EmailJSService>();
        builder.Services.AddSingleton<RecuperacionService>();
        builder.Services.AddSingleton<NegocioService>();
        builder.Services.AddSingleton<SuscripcionService>();
        builder.Services.AddSingleton<EmailJSService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CitasClienteViewModel>();
        builder.Services.AddTransient<UsuarioBasicoViewModel>();
        builder.Services.AddTransient<RegistroUsuariosViewModel>();
        builder.Services.AddTransient<SeleccionarCitaViewModel>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();
        builder.Services.AddTransient<ListaTrabjadoresViewModel>();
        builder.Services.AddTransient<NuevoTrabajadorViewModel>();
        builder.Services.AddTransient<EditarPerfilViewModel>();
        builder.Services.AddTransient<PagoSuscripcionViewModel>();
        builder.Services.AddTransient<CrearResenaViewModel>();
        builder.Services.AddTransient<CentroReseñasViewModel>();
        builder.Services.AddTransient<RecuperarContrasenaViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<Cita_ClientePage>();
        builder.Services.AddTransient<Cita_UsuarioPage>();
        builder.Services.AddTransient<CentroCalificaciones>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CrearResena>();
        builder.Services.AddTransient<RegistroUsuarioBasico>();
        builder.Services.AddTransient<RegistroUsuariosPage>();
        builder.Services.AddTransient<NuevoTrabajador>();
        builder.Services.AddTransient<ListaTrabajadores>();
        builder.Services.AddTransient<RecuperarContra>();
        builder.Services.AddTransient<NuevaContra>();
        builder.Services.AddTransient<CalendarioPage>();
        builder.Services.AddTransient<catalogoservicios>();
        builder.Services.AddTransient<CatalogoCliente>();
        builder.Services.AddTransient<DueñosNegociosPage>();
        builder.Services.AddTransient<PagoSuscripcionPage>();
        builder.Services.AddTransient<SeleccionarCitaPage>();

        return builder.Build();
    }
}