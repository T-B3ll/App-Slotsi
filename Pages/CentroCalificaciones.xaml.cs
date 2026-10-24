using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class CentroCalificaciones : ContentPage
{
    private readonly CentroReseñasViewModel _vm;
    public CentroCalificaciones(CentroReseñasViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
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