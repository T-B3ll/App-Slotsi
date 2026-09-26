using slotsi_citas.Models;
namespace slotsi_citas.Pages;

public partial class PagoSuscripcionPage : ContentPage
{

    private string _usuarioIdLogueado = "";
    public PagoSuscripcionPage()
	{
		InitializeComponent();
	}

    private string _metodoPagoSeleccionado = "Tarjeta Débito";

    private void OnMetodoPagoClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;


        BtnDebito.BackgroundColor = Colors.White;
        BtnDebito.TextColor = Color.FromArgb("#4B5563");
        BtnDebito.BorderColor = Color.FromArgb("#E5E7EB");
        BtnDebito.BorderWidth = 1;

        BtnCredito.BackgroundColor = Colors.White;
        BtnCredito.TextColor = Color.FromArgb("#4B5563");
        BtnCredito.BorderColor = Color.FromArgb("#E5E7EB");
        BtnCredito.BorderWidth = 1;


        btn.BackgroundColor = Color.FromArgb("#2563EB");
        btn.TextColor = Colors.White;
        btn.BorderColor = Colors.Transparent;
        btn.BorderWidth = 0;

        _metodoPagoSeleccionado = btn.Text.Replace("💳 ", "");

    }
    private async void OnPagarClicked(object sender, EventArgs e)
	{



        if (string.IsNullOrWhiteSpace(EntryCard.Text) ||
            string.IsNullOrWhiteSpace(EntryExpiry.Text) ||
            string.IsNullOrWhiteSpace(EntryCvv.Text) ||
            string.IsNullOrWhiteSpace(EntryName.Text))
        {
            await DisplayAlert("Campos incompletos", "Por favor llena todos los datos.", "OK");
            return;
        }

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