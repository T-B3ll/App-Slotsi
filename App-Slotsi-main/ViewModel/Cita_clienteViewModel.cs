using slotsi_citas.Models;
using App.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace slotsi_citas.ViewModels;

public class Cita_clienteViewModel : BindableObject
{
    private string _rangoSemanal = "Ago 18 - 24, 2026";

    public string RangoSemanal
    {
        get => _rangoSemanal;
        set { _rangoSemanal = value; OnPropertyChanged(); }
    }

    public ObservableCollection<RangoHorario> RangosHorarios { get; set; }

    public ICommand SemanaAnteriorCommand { get; }
    public ICommand SemanaSiguienteCommand { get; }
    public ICommand AgendarCitaClienteCommand { get; }

    public Cita_clienteViewModel()
    {
        RangosHorarios = new ObservableCollection<RangoHorario>();

        SemanaAnteriorCommand = new Command(() => { /* Lógica semana anterior */ });
        SemanaSiguienteCommand = new Command(() => { /* Lógica semana siguiente */ });
        AgendarCitaClienteCommand = new Command<RangoHorario>(AgendarCita);

        CargarHorarios();
    }

    private void CargarHorarios()
    {
        RangosHorarios.Clear();

        RangosHorarios.Add(new RangoHorario { HoraDisplay = "8:00 AM", Cita = null });
        RangosHorarios.Add(new RangoHorario { HoraDisplay = "9:00 AM", Cita = null });
        RangosHorarios.Add(new RangoHorario { HoraDisplay = "10:00 AM", Cita = null });
        RangosHorarios.Add(new RangoHorario { HoraDisplay = "11:00 AM", Cita = null });

        // Slot Ocupado
        RangosHorarios.Add(new RangoHorario
        {
            HoraDisplay = "12:00 PM",
            Cita = new Cita_cliente { Estado = "Ocupado", NombreCliente = "(12:00 - 1:00)" }
        });

        // Slot No disponible
        RangosHorarios.Add(new RangoHorario
        {
            HoraDisplay = "1:00 PM",
            Cita = new Cita_cliente { Estado = "NoDisponible" }
        });

        RangosHorarios.Add(new RangoHorario { HoraDisplay = "2:00 PM", Cita = null });
        RangosHorarios.Add(new RangoHorario { HoraDisplay = "3:00 PM", Cita = null });
        RangosHorarios.Add(new RangoHorario { HoraDisplay = "4:00 PM", Cita = null });
    }

    private async void AgendarCita(RangoHorario rango)
    {
        if (rango != null && Application.Current?.Windows[0].Page is Page mainPage)
        {
            await mainPage.DisplayAlert("Agendar Cita", $"Seleccionaste el horario: {rango.HoraDisplay}", "OK");
        }
    }

    public void RefrescarCitasCliente() => CargarHorarios();
}