namespace slotsi_citas.Pages;

public partial class SeleccionarCitaPage : ContentPage
{
    public SeleccionarCitaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Restablece la opacidad y posición al volver a entrar
        this.Opacity = 1;
        this.TranslationY = 0;
    }
}