using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace slotsi_citas.Models
{
    public class Trabajador : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string NegocioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Foto { get; set; } = string.Empty;
        public string ArchivoProfesional { get; set; } = string.Empty;

        private bool _activo;
        public bool Activo
        {
            get => _activo;
            set
            {
                if (_activo != value)
                {
                    _activo = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<HorarioTrabajo> HorarioTrabajo { get; set; } = new();

        private bool _esSeleccionado;
        public bool EsSeleccionado
        {
            get => _esSeleccionado;
            set
            {
                if (_esSeleccionado != value)
                {
                    _esSeleccionado = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColorBorde));
                    OnPropertyChanged(nameof(ColorFondo));
                    OnPropertyChanged(nameof(ColorTexto));
                }
            }
        }

        public string ColorBorde => EsSeleccionado ? "#2563EB" : "#E2E8F0";
        public string ColorFondo => EsSeleccionado ? "#EFF6FF" : "#FFFFFF";
        public string ColorTexto => EsSeleccionado ? "#2563EB" : "#0F172A";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HorarioTrabajo
    {
        public string Dia { get; set; } = string.Empty;
        public string Inicio { get; set; } = string.Empty;
        public string Fin { get; set; } = string.Empty;
    }
}