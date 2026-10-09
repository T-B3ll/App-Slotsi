using slotsi_citas.ViewModel;
using slotsi_citas.Services;

namespace slotsi_citas.Pages;

public partial class CalendarioPage : ContentPage
{
    public CalendarioPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refrescar los estados de los bloques de hora al volver a la pantalla
        if (BindingContext is CalendarioViewModel vm)
        {
            if (vm.RangosHorarios != null)
            {
                foreach (var rango in vm.RangosHorarios)
                {
                    if (CitaRepository.CitasRegistradas.TryGetValue(rango.HoraDisplay, out var cita))
                    {
                        rango.Cita = cita;
                        rango.EsOcupadoManual = true;
                    }
                }
            }
        }
    }
}