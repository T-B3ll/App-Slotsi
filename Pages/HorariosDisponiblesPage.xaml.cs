using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class HorariosDisponiblesPage : ContentPage
{
    private readonly HorariosDisponiblesViewModel _viewModel;

    public HorariosDisponiblesPage(HorariosDisponiblesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // ApplyQueryAttributes ya se encarga de cargar los horarios
    }
}