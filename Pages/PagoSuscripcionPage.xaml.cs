namespace slotsi_citas.Pages;

public partial class PagoSuscripcionPage : ContentPage
{
	public PagoSuscripcionPage()
	{
		InitializeComponent();
	}

    private async void OnPagarClicked(object sender, EventArgs e)
	{
        var btn = (Button)sender;
        btn.IsEnabled = false;
        btn.Text = "Procesando...";
        await Task.Delay(1500);

        try
        {
            await DisplayAlert("¡Pago Exitoso!",
               "Tu suscripción está activa hasta el 19 Oct, 2026.\n\n(Nota: Este es un pago simulado para demo)",
               "Genial");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo procesar: {ex.Message}", "OK");
        }
        finally
        {
            btn.IsEnabled = true;
            btn.Text = "Pagar $5 USD";
        }
    }
}