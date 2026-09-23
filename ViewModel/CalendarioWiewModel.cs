using System.Collections.ObjectModel;
using System.Windows.Input;
using slotsi_citas.Models;

namespace slotsi_citas.ViewModel;

public class CalendarioViewModel
{
    public string RangoFecha { get; set; } = "Ago 18 - 24, 2026";
    public ObservableCollection<DiaSemanaItem> DiasSemana { get; set; }
    public ObservableCollection<CitaCalendarioItem> Citas { get; set; }

    public ICommand HoyCommand { get; }
    public ICommand AnterioresCommand { get; }
    public ICommand SiguientesCommand { get; }

    public CalendarioViewModel()
    {
        // Días de la semana
        DiasSemana = new ObservableCollection<DiaSemanaItem>
        {
            new DiaSemanaItem { LetraDia = "L", NumeroDia = "17", EsSeleccionado = false },
            new DiaSemanaItem { LetraDia = "M", NumeroDia = "18", EsSeleccionado = false },
            new DiaSemanaItem { LetraDia = "Mi", NumeroDia = "19", EsSeleccionado = false },
            new DiaSemanaItem { LetraDia = "J", NumeroDia = "20", EsSeleccionado = false },
            new DiaSemanaItem { LetraDia = "V", NumeroDia = "21", EsSeleccionado = true },
            new DiaSemanaItem { LetraDia = "S", NumeroDia = "22", EsSeleccionado = false },
            new DiaSemanaItem { LetraDia = "D", NumeroDia = "23", EsSeleccionado = false }
        };

        // Citas programadas
        Citas = new ObservableCollection<CitaCalendarioItem>
        {
            new CitaCalendarioItem
            {
                ClienteNombre = "María González",
                ServicioYHora = "Corte Clásico (9:00 - 10:00)",
                HoraInicio = "9:00 AM",
                ColorBarra = "#00C781",
                ColorFondo = "#EDFAF4"
            },
            new CitaCalendarioItem
            {
                ClienteNombre = "Carlos Mendoza",
                ServicioYHora = "Afeitado de Barba (11:00 - 11:30)",
                HoraInicio = "11:00 AM",
                ColorBarra = "#FF9500",
                ColorFondo = "#FFF8ED"
            }
        };

        HoyCommand = new Command(async () =>
            await Application.Current.MainPage.DisplayAlert("Calendario", "Día actual seleccionado", "OK"));

        AnterioresCommand = new Command(() => { });
        SiguientesCommand = new Command(() => { });
    }
}