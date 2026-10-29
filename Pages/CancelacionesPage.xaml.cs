using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class CancelacionesPage : ContentPage
{
	private readonly CancelacionesViewModel _viewModel;
    public CancelacionesPage(CancelacionesViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
        BindingContext = _viewModel;
    }
    private async void OnVolverClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarCommand.ExecuteAsync(null);
    }
}