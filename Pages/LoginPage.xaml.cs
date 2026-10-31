using Microsoft.Extensions.DependencyInjection;
using slotsi_citas.Services;
using slotsi_citas.Services.Api;
using slotsi_citas.Services.Auth;

namespace slotsi_citas.Pages;

public partial class LoginPage : ContentPage
{
    private readonly IAuthApiService _auth;
    private readonly ISesionService _sesion;
    private readonly UsuarioService _usuarioService;
    private readonly NegocioService _negocioService;

    // Evita doble toque
    private bool _isNavigating = false;

    public LoginPage(IAuthApiService auth, ISesionService sesion,
                     UsuarioService usuarioService, NegocioService negocioService)
    {
        InitializeComponent();
        _auth = auth;
        _sesion = sesion;
        _usuarioService = usuarioService;
        _negocioService = negocioService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_sesion.EstaAutenticado || await _sesion.RestaurarAsync())
        {
            bool esDueno = Preferences.Get("EsDuenoNegocio", false);
            await IrAlShellAsync(esDueno);
        }
    }

    private async Task IrAlShellAsync(bool esDueno)
    {
        var app = Application.Current!;
        var shell = new AppShell(_auth);

        if (app.Windows.Count > 0)
            app.Windows[0].Page = shell;
        else
            app.MainPage = shell;

        await shell.GoToAsync(esDueno ? "//ResumenPage" : "//dueñosnegocios");
    }

    private async void OnIniciarSesionClicked(object? sender, EventArgs e)
    {
        var email = entryUsuario.Text?.Trim().ToLowerInvariant();
        var contrasena = entryContrasena.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(contrasena))
        {
            await DisplayAlert("Faltan datos", "Escribe tu correo y tu contraseña.", "Entendido");
            return;
        }

        Cargando(true);
        try
        {
            await _auth.IniciarSesionAsync(email, contrasena);
            entryContrasena.Text = string.Empty;
            var usuario = await _usuarioService.ObtenerPorCorreoAsync(email);
            if (usuario == null)
            {
                await _auth.CerrarSesionAsync();
                await DisplayAlert("Error", "No se encontró el usuario.", "Entendido");
                return;
            }

            string idUsuario = usuario.Id?.ToString() ?? "";
            Preferences.Set("UsuarioId", idUsuario);
            Preferences.Set("UsuarioIdSesion", idUsuario);
            Preferences.Set("EsDuenoNegocio", usuario.TipoUsuario);
            Preferences.Set("UsuarioCorreo", usuario.Correo);

            Preferences.Remove("negocio_id");
            if (usuario.TipoUsuario)
            {
                try
                {
                    var negocio = await _negocioService.ObtenerPorUsuarioIdAsync(idUsuario);
                    if (!string.IsNullOrEmpty(negocio?.Id))
                        Preferences.Set("negocio_id", negocio.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"No se pudo obtener el negocio: {ex.Message}");
                }
            }
            await IrAlShellAsync(usuario.TipoUsuario);
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo iniciar sesión", ApiException.MensajePara(ex), "Entendido");
        }
        finally
        {
            Cargando(false);
        }
    }

    private void Cargando(bool activo)
    {
        botonIniciarSesion.IsEnabled = !activo;
        indicadorCarga.IsVisible = indicadorCarga.IsRunning = activo;
    }

    private async void OnForgotPasswordClicked(object? sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            await Task.Yield(); 

            var pagina = Handler?.MauiContext?.Services.GetRequiredService<RecuperarContra>();
            if (pagina != null)
                await Navigation.PushAsync(pagina, false); 
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async void OnCreateAccountClicked(object? sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            await Task.Yield(); 

            var pagina = Handler?.MauiContext?.Services.GetService<OpcionRegistrosU>() ?? new OpcionRegistrosU();
            await Navigation.PushAsync(pagina, false); 
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private void OnTogglePasswordVisibility(object? sender, EventArgs e) =>
        entryContrasena.IsPassword = !entryContrasena.IsPassword;
}