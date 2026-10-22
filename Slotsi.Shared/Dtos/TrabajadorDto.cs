using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotsi.Shared.Dtos
{
    public class CambiarEstadoDto { public bool Activo { get; set; } }

    public class HorarioTrabajoDto
    {
        public string Dia { get; set; } = "";
        public string Inicio { get; set; } = "";
        public string Fin { get; set; } = "";
    }
    public class TrabajadorDto
    {
        public string? Id { get; set; }
        public string NegocioId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Especialidad { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Foto { get; set; } = "";
        public string ArchivoProfesional { get; set; } = "";
        public bool Activo { get; set; }
        public List<HorarioTrabajoDto> HorarioTrabajo { get; set; } = new();
    }
}
