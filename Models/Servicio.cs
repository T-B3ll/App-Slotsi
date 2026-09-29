using System.ComponentModel;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace slotsi_citas.Models
{
    [BsonIgnoreExtraElements]
    public class Servicio : INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [BsonElement("precio")]
        public double Precio { get; set; }

        [BsonElement("duracion_minutos")]
        public int DuracionMinutos { get; set; }

        [BsonElement("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [BsonElement("foto")]
        public string Foto { get; set; } = string.Empty;

        [BsonElement("disponible_domicilio")]
        public bool DisponibleDomicilio { get; set; }

        // Propiedad local para UI
        private bool _esSeleccionado;
        [BsonIgnore]
        public bool EsSeleccionado
        {
            get => _esSeleccionado;
            set
            {
                if (_esSeleccionado != value)
                {
                    _esSeleccionado = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TextoBoton));
                    OnPropertyChanged(nameof(ColorFondoBoton));
                    OnPropertyChanged(nameof(ColorTextoBoton));
                    OnPropertyChanged(nameof(ColorBordeCard));
                }
            }
        }

        [BsonIgnore]
        public string DuracionTexto => $"{DuracionMinutos} min";

        [BsonIgnore]
        public string PrecioTexto => $"C$ {Precio:N2}";

        [BsonIgnore]
        public string TextoBoton => EsSeleccionado ? "✓ Seleccionado" : "+ Seleccionar";

        [BsonIgnore]
        public string ColorFondoBoton => EsSeleccionado ? "#2563EB" : "#FFFFFF";

        [BsonIgnore]
        public string ColorTextoBoton => EsSeleccionado ? "#FFFFFF" : "#2563EB";

        [BsonIgnore]
        public string ColorBordeCard => EsSeleccionado ? "#2563EB" : "#E5E7EB";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}