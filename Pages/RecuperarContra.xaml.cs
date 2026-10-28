namespace slotsi_citas.Pages;

using slotsi_citas.Services;
using slotsi_citas.Services.Api;
using Microsoft.Maui.Controls;

public partial class RecuperarContra : ContentPage
{
    private readonly IAuthApiService _authService;
    private readonly EmailJSService _emailService;

    public RecuperarContra(IAuthApiService authService, EmailJSService emailService)
    {
        InitializeComponent();
        _authService = authService;
        _emailService = emailService;
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private async void OnEnviarCodigoClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCorreo.Text))
        {
            await DisplayAlert("Error", "Ingresa tu correo.", "OK");
            return;
        }

        try
        {
            var correo = EntryCorreo.Text.Trim().ToLower();

            // 1. Pedimos el token a la API (El token se guarda en la colección Usuarios)
            var respuesta = await _authService.SolicitarRestablecimientoAsync(correo);

            if (string.IsNullOrEmpty(respuesta.Token))
            {
                await DisplayAlert("Aviso", "Si el correo está registrado, recibirás un código.", "OK");
                return;
            }

         
            var enviado = await _emailService.EnviarCodigoPorEmail(correo, respuesta.Token);

            if (enviado)
            {
                EntryCodigo.Text = string.Empty; 
                await DisplayAlert("Éxito", $"Código enviado a {correo}. Revisa tu Gmail.", "OK");
                EntryCodigo.Focus();
            }
            else
            {
                await DisplayAlert("Error", "El correo no pudo ser enviado. Intenta de nuevo.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSeguirClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCodigo.Text))
        {
            await DisplayAlert("Error", "Ingresa el código que recibiste.", "OK");
            return;
        }

        
        await Navigation.PushAsync(new NuevaContra(EntryCorreo.Text.Trim().ToLower(), EntryCodigo.Text.Trim()));
    }
}