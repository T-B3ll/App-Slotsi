using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace slotsi_citas.Models
{
    public class Servicios : INotifyPropertyChanged
    {
        private string? _id;
        [JsonPropertyName("id")]
        public string? Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        private string _negocioId = string.Empty;
        [JsonPropertyName("negocioId")]
        public string NegocioId
        {
            get => _negocioId;
            set { _negocioId = value; OnPropertyChanged(); }
        }

        private string _nombre = string.Empty;
        [JsonPropertyName("nombre")]
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        private string _descripcion = string.Empty;
        [JsonPropertyName("descripcion")]
        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(); }
        }

        private int _duracionMinutos;
        [JsonPropertyName("duracionMinutos")]
        public int DuracionMinutos
        {
            get => _duracionMinutos;
            set { _duracionMinutos = value; OnPropertyChanged(); }
        }

        private decimal _precio;
        [JsonPropertyName("precio")]
        public decimal Precio
        {
            get => _precio;
            set { _precio = value; OnPropertyChanged(); }
        }

        private string _categoria = string.Empty;
        [JsonPropertyName("categoria")]
        public string Categoria
        {
            get => _categoria;
            set { _categoria = value; OnPropertyChanged(); }
        }

        private List<string> _colaboradoresHabilitados = new();
        [JsonPropertyName("colaboradoresHabilitados")]
        public List<string> ColaboradoresHabilitados
        {
            get => _colaboradoresHabilitados;
            set { _colaboradoresHabilitados = value; OnPropertyChanged(); }
        }

        private bool _disponibleDomicilio;
        [JsonPropertyName("disponibleDomicilio")]
        public bool DisponibleDomicilio
        {
            get => _disponibleDomicilio;
            set { _disponibleDomicilio = value; OnPropertyChanged(); }
        }

        private bool _activo;
        [JsonPropertyName("activo")]
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

        private string _foto = string.Empty;
        [JsonPropertyName("foto")]
        public string Foto
        {
            get => _foto;
            set { _foto = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneFoto)); OnPropertyChanged(nameof(NoTieneFoto)); }
        }

        public ObservableCollection<Trabajador> ColaboradoresDelServicio { get; set; } = new();

        private Trabajador? _colaboradorSeleccionado;
        public Trabajador? ColaboradorSeleccionado
        {
            get => _colaboradorSeleccionado;
            set
            {
                if (_colaboradorSeleccionado != value)
                {
                    _colaboradorSeleccionado = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColaboradorSeleccionadoNombre));
                }
            }
        }

        public string ColaboradorSeleccionadoNombre => ColaboradorSeleccionado != null ? ColaboradorSeleccionado.Nombre : "Seleccionar especialista";

        public bool TieneFoto => !string.IsNullOrEmpty(Foto);
        public bool NoTieneFoto => string.IsNullOrEmpty(Foto);
        public string ModalidadTexto => DisponibleDomicilio ? "🏠 A Domicilio" : "💈 En Local";

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
                    OnPropertyChanged(nameof(ColorBordeCard));
                    OnPropertyChanged(nameof(ColorFondoBoton));
                    OnPropertyChanged(nameof(ColorTextoBoton));
                    OnPropertyChanged(nameof(TextoBoton));
                }
            }
        }

        public string ColorBordeCard => EsSeleccionado ? "#2563EB" : "#E5E7EB";
        public string ColorFondoBoton => EsSeleccionado ? "#2563EB" : "#FFFFFF";
        public string ColorTextoBoton => EsSeleccionado ? "#FFFFFF" : "#2563EB";
        public string TextoBoton => EsSeleccionado ? "Quitar" : "Agregar";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}