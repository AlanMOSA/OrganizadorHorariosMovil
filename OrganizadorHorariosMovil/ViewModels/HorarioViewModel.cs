using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrganizadorHorariosMovil.Models;

namespace OrganizadorHorariosMovil.ViewModels
{
    public class HorarioViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Materia> _materias = new ObservableCollection<Materia>(); // Initialize to avoid null
        public ObservableCollection<Materia> Materias
        {
            get => _materias;
            set
            {
                _materias = value;
                OnPropertyChanged(nameof(Materias));
            }
        }

        private int _materiasCount;
        public int MateriasCount
        {
            get => _materiasCount;
            set
            {
                _materiasCount = value;
                OnPropertyChanged(nameof(MateriasCount));
            }
        }

        public string[] Dias { get; } = { "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES" };
        public string[] Horas { get; } = {
                    "7:00 a 7:50", "7:50 a 8:40", "8:40 a 9:30", "9:30 a 10:20",
                    "10:40 a 11:30", "11:30 a 12:20", "12:20 a 13:10", "13:10 a 14:00"
                };

        public HorarioViewModel()
        {
            Materias = new ObservableCollection<Materia>();
            ActualizarContador();
        }

        public void AgregarMateria(Materia materia)
        {
            Materias.Add(materia);
            ActualizarContador();
        }

        public void LimpiarHorario()
        {
            Materias.Clear();
            ActualizarContador();
        }

        private void ActualizarContador()
        {
            MateriasCount = Materias.Count;
        }

        public event PropertyChangedEventHandler? PropertyChanged; // Updated to match nullable reference type

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
