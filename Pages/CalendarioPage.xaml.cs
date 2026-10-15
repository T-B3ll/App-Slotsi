using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class CitasClienteViewModel : BindableObject
    {
        private readonly MongoService _mongoService;
        private DateTime _fechaSeleccionada;

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (_fechaSeleccionada != value)
                {
                    _fechaSeleccionada = value;
                    OnPropertyChanged();
                    _ = CargarRangosHorariosAsync();
                }
            }
        }

        public ObservableCollection<RangoHorario> RangosHorarios { get; } = new ObservableCollection<RangoHorario>();

        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand CambiarAFechaDeHoyCommand { get; }
        public ICommand AgendarCitaClienteCommand { get; }

        // Constructor con Inyección de Dependencias
        public CitasClienteViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;

    }
}