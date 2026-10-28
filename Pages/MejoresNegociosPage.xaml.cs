using slotsi_citas.ViewModel;
using slotsi_citas.Models;

namespace slotsi_citas.Pages;

public partial class MejoresNegociosPage : ContentPage
{
    private readonly MejoresNegociosViewModel _viewModel;
    public MejoresNegociosPage(MejoresNegociosViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }

    private void OnCategoriaClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        MarcarActivo(btn, BtnTodos, BtnSalones, BtnBarberia, BtnSpa);
        _viewModel.CambiarCategoria(btn.Text);
    }

    private void OnTopClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || !int.TryParse(btn.ClassId, out int top)) return;
        MarcarActivo(btn, BtnTop10, BtnTop20, BtnTop50, BtnTop100);
        _viewModel.CambiarTop(top);
    }

    private static void MarcarActivo(Button activo, params Button[] grupo)
    {
        foreach (var b in grupo)
        {
            b.BackgroundColor = Colors.White;
            b.TextColor = Color.FromArgb("#4B5563");
            b.FontAttributes = FontAttributes.None;
        }
        activo.BackgroundColor = Color.FromArgb("#2563EB");
        activo.TextColor = Colors.White;
        activo.FontAttributes = FontAttributes.Bold;
    }

    private async void OnNegocioTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not NegocioRanckingItem item) return;

        string? id = item.Negocio.Id;
        if (string.IsNullOrEmpty(id))
        {
            await DisplayAlert("Aviso", "Este negocio no tiene un ID registrado.", "OK");
            return;
        }

        Preferences.Set("NegocioSeleccionadoId", id);
        await Shell.Current.GoToAsync(nameof(CatalogoCliente));
    }
}