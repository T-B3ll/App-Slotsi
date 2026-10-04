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