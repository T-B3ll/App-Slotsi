using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;

namespace slotsi_citas.ViewModel;

public class CalendarioViewModel : BindableObject
{
    public string RangoFecha { get; set; } = "Ago 18 - 24, 2026";
    public ObservableCollection<DiaSemanaItem> DiasSemana { get; set; }

    // Colección de bloques de hora para el calendario
    public ObservableCollection<RangoHorario> RangosHorarios { get; set; }

    public ICommand HoyCommand { get; }
    public ICommand AnterioresCommand { get; }
    public ICommand SiguientesCommand { get; }
    public ICommand SeleccionarHorarioCommand { get; }

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

        // Inicializar los bloques de horarios
        RangosHorarios = new ObservableCollection<RangoHorario>
        {
            new RangoHorario { HoraDisplay = "8:00 AM" },
            new RangoHorario { HoraDisplay = "9:00 AM" },
            new RangoHorario { HoraDisplay = "10:00 AM" },
            new RangoHorario { HoraDisplay = "11:00 AM" },
            new RangoHorario { HoraDisplay = "12:00 PM" },
            new RangoHorario { HoraDisplay = "1:00 PM", EsOcupadoManual = true }, // Bloque de almuerzo/descanso
            new RangoHorario { HoraDisplay = "2:00 PM" },
            new RangoHorario { HoraDisplay = "3:00 PM" },
            new RangoHorario { HoraDisplay = "4:00 PM" }
        };

        // Comando para seleccionar horario y navegar hacia SeleccionarCitaPage
        SeleccionarHorarioCommand = new Command<RangoHorario>(async (horario) =>
        {
            if (horario == null || horario.EsOcupado) return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "HorarioSeleccionado", horario }
            };

            await Shell.Current.GoToAsync("SeleccionarCitaPage", navigationParameters);
        });

        HoyCommand = new Command(async () =>
            await Application.Current.MainPage.DisplayAlert("Calendario", "Día actual seleccionado", "OK"));

        AnterioresCommand = new Command(() => { });
        SiguientesCommand = new Command(() => { });
    }
}