using App.Models;
using Microsoft.Extensions.DependencyInjection;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace slotsi_citas.ViewModels;

public class Cita_clienteViewModel : BindableObject
{
    private readonly IServiceProvider? _serviceProvider;
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

    public Cita_clienteViewModel() : this(null)
    {
    }

    public Cita_clienteViewModel(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RangosHorarios = new ObservableCollection<RangoHorario>();

        SemanaAnteriorCommand = new Command(() => { });
        SemanaSiguienteCommand = new Command(() => { });
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

    private async void AgendarCita(RangoHorario? rango)
    {
        if (rango == null || !rango.EsDisponible)
            return;

        string direccionGuardada = Preferences.Default.Get("direccion_guardada", "Matagalpa, Nicaragua");

        var parametros = new Dictionary<string, object>
        {
            { "HorarioSeleccionado", rango },
            { "DireccionCliente", direccionGuardada }
        };

        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(nameof(SeleccionarCitaPage), parametros);
            }
            else
            {
                NavegarFallback(rango, direccionGuardada);
            }
        }
        catch
        {
            NavegarFallback(rango, direccionGuardada);
        }
    }

    private async void NavegarFallback(RangoHorario rango, string direccionGuardada)
    {
        var paginaDestino = _serviceProvider?.GetService<SeleccionarCitaPage>() ?? new SeleccionarCitaPage();

        if (paginaDestino.BindingContext is SeleccionarCitaViewModel vm)
        {
            vm.HorarioSeleccionado = rango;
            vm.DireccionCliente = direccionGuardada;
        }

        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.Page?.Navigation != null)
        {
            await window.Page.Navigation.PushAsync(paginaDestino);
        }
    }

    // MÉTODO PÚBLICO
    public void RefrescarCitasCliente()
    {
        CargarHorarios();
    }
}