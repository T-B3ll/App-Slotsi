using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.ViewModel
{
    public class HorarioTrabajoVm : INotifyPropertyChanged
    {
            private bool _esLaboral = true;
            private TimeSpan _horaInicio = new TimeSpan(8, 0, 0);
            private TimeSpan _horaFin = new TimeSpan(17, 0, 0);

            public string Dia { get; set; }

            public bool EsLaboral
            {
                get => _esLaboral;
                set { _esLaboral = value; OnPropertyChanged(); }
            }

            public TimeSpan HoraInicio
            {
                get => _horaInicio;
                set { _horaInicio = value; OnPropertyChanged(); }
            }

            public TimeSpan HoraFin
            {
                get => _horaFin;
                set { _horaFin = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
    }
}
