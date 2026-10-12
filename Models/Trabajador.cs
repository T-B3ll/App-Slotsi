using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace slotsi_citas.Models
{
    [BsonIgnoreExtraElements]
    public class Trabajador : INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("negocio_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string NegocioId { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("especialidad")]
        public string Especialidad { get; set; }

        [BsonElement("correo")]
        public string Correo { get; set; }

        [BsonElement("telefono")]
        public string Telefono { get; set; }

        [BsonElement("cedula")]
        public string Cedula { get; set; }

        [BsonElement("foto")]
        public string Foto { get; set; }

        [BsonElement("archivoprofesional")]
        public string ArchivoProfesional { get; set; }

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

        [BsonElement("horario_trabajo")]
        public List<HorarioTrabajo> HorarioTrabajo { get; set; }

<<<<<<< HEAD
=======
        // --- PROPIEDADES AUXILIARES DE UI PARA SELECCIÓN ---
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
                    OnPropertyChanged(nameof(ColorBorde));
                    OnPropertyChanged(nameof(ColorFondo));
                    OnPropertyChanged(nameof(ColorTexto));
                }
            }
        }

        [BsonIgnore]
        public bool TieneFoto => !string.IsNullOrEmpty(Foto);

        [BsonIgnore]
        public bool NoTieneFoto => string.IsNullOrEmpty(Foto);

        [BsonIgnore]
        public string ColorBorde => EsSeleccionado ? "#2563EB" : "#E2E8F0";

        [BsonIgnore]
        public string ColorFondo => EsSeleccionado ? "#EFF6FF" : "#FFFFFF";

        [BsonIgnore]
        public string ColorTexto => EsSeleccionado ? "#2563EB" : "#0F172A";

>>>>>>> catalogos
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HorarioTrabajo
    {
        [BsonElement("dia")]
        public string Dia { get; set; }

        [BsonElement("inicio")]
        public string Inicio { get; set; }

        [BsonElement("fin")]
        public string Fin { get; set; }
    }
}