using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizadorHorariosMovil.Models
{
    public class Materia  // PUBLIC agregado
    {
        public string Nombre { get; set; } = string.Empty;
        public string Maestro { get; set; } = string.Empty;
        public string Dia { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty; // Initialize to avoid nullability warning
        public int Duracion { get; set; }

        public override string ToString()
        {
            return $"{Nombre}\n{Maestro}";
        }
    }
}


