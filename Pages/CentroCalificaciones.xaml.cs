using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class CentroCalificaciones : ContentPage
{
	private readonly CentroReseñasViewModel _vm;
    public CentroCalificaciones()
	{
		InitializeComponent();
        _vm = new CentroReseñasViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm != null)
        {
            await _vm.CargarCalificacionesAsync();
        }
    }
}