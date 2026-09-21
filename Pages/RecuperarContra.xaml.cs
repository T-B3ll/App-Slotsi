namespace slotsi_citas.Pages;
using slotsi_citas.Services;
using Microsoft.Maui.Controls;
public partial class RecuperarContra : ContentPage
{

    private readonly RecuperacionService _recuperacionService;

    public RecuperarContra(RecuperacionService recuperacionService)
    {
        InitializeComponent();
        _recuperacionService = recuperacionService;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnEnviarCodigoClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCorreo.Text))
        {
            await DisplayAlert("Error", "Ingresa tu correo.", "OK");
            return;
        }

        try
        {
         
            var codigo = await _recuperacionService.GenerarSolicitudAsync(EntryCorreo.Text);

         
            var emailService = new EmailJSService();
            var enviado = await emailService.EnviarCodigoPorEmail(EntryCorreo.Text, codigo);

            if (enviado)
            {
                await DisplayAlert("Éxito", $"Código enviado a {EntryCorreo.Text}\nRevisa tu bandeja (y spam).", "OK");
                EntryCodigo.Focus();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo enviar el correo. Intenta más tarde.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSeguirClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCodigo.Text) || EntryCodigo.Text.Length != 6)
        {
            await DisplayAlert("Error", "Por favor ingresa el código de 6 dígitos.", "OK");
            return;
        }

        try
        {
           
            var solicitudValida = await _recuperacionService.ValidarCodigoAsync(
                EntryCorreo.Text,
                EntryCodigo.Text
            );

            if (solicitudValida != null)
            {
             
                await Navigation.PushAsync(new NuevaContra(solicitudValida.Id, EntryCorreo.Text));
            }
            else
            {
                await DisplayAlert("Error", "El código es incorrecto o ha expirado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

    }
}