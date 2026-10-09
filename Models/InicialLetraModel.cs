using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace slotsi_citas.Models
{
    public class InicialLetraModel : INotifyPropertyChanged
    {
        private string _letra = string.Empty;
        private bool _esSeleccionada;

        public string Letra
        {
            get => _letra;
            set
            {
                if (_letra != value)
                {
                    _letra = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EsSeleccionada
        {
            get => _esSeleccionada;
            set
            {
                if (_esSeleccionada != value)
                {
                    _esSeleccionada = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}