namespace slotsi_citas.Pages;

public partial class RecuperarContra : ContentPage
{
    public RecuperarContra()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnEnviarCodigoClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCorreo.Text))
        {
            await DisplayAlert("Error", "Por favor ingresa tu correo electrónico.", "OK");
            return;
        }

        await DisplayAlert("Código Enviado", $"Hemos enviado un código a {EntryCorreo.Text}", "OK");
        EntryCodigo.Focus();
    }

    private async void OnSeguirClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCodigo.Text) || EntryCodigo.Text.Length < 6)
        {
            await DisplayAlert("Error", "Por favor ingresa el código de 6 dígitos.", "OK");
            return;
        }

       await Navigation.PushAsync(new NuevaContra());
    }
}