using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace slotsi_citas.Models
{
    [BsonIgnoreExtraElements]
    public class Servicios : INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("negocio_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string NegocioId { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [BsonElement("duracion_minutos")]
        public int DuracionMinutos { get; set; }

        [BsonElement("precio")]
        public decimal Precio { get; set; }

        [BsonElement("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [BsonElement("colaboradores_habilitados")]
        public List<ObjectId> ColaboradoresHabilitados { get; set; } = new();

        [BsonElement("disponible_domicilio")]
        public bool DisponibleDomicilio { get; set; }

        private bool _activo;

        [BsonElement("activo")]
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

        [BsonElement("foto")]
        public string Foto { get; set; } = string.Empty;

        [BsonIgnore]
        public bool TieneFoto => !string.IsNullOrEmpty(Foto);

        [BsonIgnore]
        public bool NoTieneFoto => string.IsNullOrEmpty(Foto);

        [BsonIgnore]
        public string ModalidadTexto => DisponibleDomicilio ? "🏠 A Domicilio" : "💈 En Local";

        [BsonIgnore]
        public string DuracionTexto => $"{DuracionMinutos} min";

        [BsonIgnore]
        public string PrecioTexto => $"C$ {Precio:N2}";

        // --- PROPIEDADES DE SELECCIÓN PARA LA VISTA ---
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
        public string ColorBordeCard => EsSeleccionado ? "#2563EB" : "#E5E7EB";

        [BsonIgnore]
        public string ColorFondoBoton => EsSeleccionado ? "#2563EB" : "#FFFFFF";

        [BsonIgnore]
        public string TextoBoton => EsSeleccionado ? "Seleccionado" : "Seleccionar";

        [BsonIgnore]
        public string ColorTextoBoton => EsSeleccionado ? "#FFFFFF" : "#2563EB";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}