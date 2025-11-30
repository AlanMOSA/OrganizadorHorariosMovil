using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrganizadorHorariosMovil.Models;
using OrganizadorHorariosMovil.ViewModels;

namespace OrganizadorHorariosMovil.Views
{

    public partial class AgregarMateriaPage : ContentPage
    {
        
        private HorarioViewModel _viewModel;
        private string[] _horasCompletas = {
            "7:00 a 7:50", "7:50 a 8:40", "8:40 a 9:30", "9:30 a 10:20",
            "10:40 a 11:30", "11:30 a 12:20", "12:20 a 13:10", "13:10 a 14:00"
        };

        public AgregarMateriaPage(HorarioViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            // Configurar valores por defecto
            cmbDuracion.SelectedIndex = 0;
        }

        private void OnHoraInicioChanged(object sender, EventArgs e)
        {
            ActualizarHoraFin();
        }

        private void OnDuracionChanged(object sender, EventArgs e)
        {
            ActualizarHoraFin();
        }

        private void ActualizarHoraFin()
        {
            if (cmbHoraInicio.SelectedIndex == -1 || cmbDuracion.SelectedIndex == -1)
                return;

            int inicioIndex = cmbHoraInicio.SelectedIndex;
            int duracion = cmbDuracion.SelectedIndex + 1; // 1, 2 o 3 horas
            int finIndex = inicioIndex + duracion - 1;

            if (finIndex < _horasCompletas.Length && finIndex >= 0)
            {
                string horaFinCompleta = _horasCompletas[finIndex];
                string[] partes = horaFinCompleta.Split(' ');
                string horaFinSimple = partes[2]; // "7:50"

                lblHoraFin.Text = $"Hora de fin: {horaFinSimple}";
            }
            else
            {
                lblHoraFin.Text = "Hora de fin: --";
            }
        }

        private async void OnAgregarClicked(object sender, EventArgs e)
        {
            if (ValidarDatos())
            {
                var materia = new Materia
                {
                    Nombre = txtMateria.Text.Trim(),
                    Maestro = txtMaestro.Text.Trim(),
                    Dia = cmbDia.SelectedItem?.ToString() ?? string.Empty, // Fix for CS8601
                    HoraInicio = ExtraerHoraInicio(cmbHoraInicio.SelectedItem?.ToString() ?? ""), // Fix for CS8604
                    HoraFin = ExtraerHoraFin(lblHoraFin.Text),
                    Duracion = cmbDuracion.SelectedIndex + 1
                };

                _viewModel.AgregarMateria(materia);
                await DisplayAlert("Éxito", "Materia agregada correctamente", "OK");
                await Navigation.PopAsync();
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtMateria.Text))
            {
                DisplayAlert("Error", "Por favor ingresa el nombre de la materia", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMaestro.Text))
            {
                DisplayAlert("Error", "Por favor ingresa el nombre del maestro", "OK");
                return false;
            }

            if (cmbDia.SelectedIndex == -1)
            {
                DisplayAlert("Error", "Por favor selecciona un día", "OK");
                return false;
            }

            if (cmbHoraInicio.SelectedIndex == -1)
            {
                DisplayAlert("Error", "Por favor selecciona la hora de inicio", "OK");
                return false;
            }

            if (cmbDuracion.SelectedIndex == -1)
            {
                DisplayAlert("Error", "Por favor selecciona la duración", "OK");
                return false;
            }

            // Validar que la duración no exceda el horario
            int inicioIndex = cmbHoraInicio.SelectedIndex;
            int duracion = cmbDuracion.SelectedIndex + 1;
            int finIndex = inicioIndex + duracion - 1;

            if (finIndex >= _horasCompletas.Length)
            {
                DisplayAlert("Error", "La duración seleccionada excede el horario disponible", "OK");
                return false;
            }

            return true;
        }

        private string ExtraerHoraInicio(string? horaCompleta)
        {
            if (string.IsNullOrEmpty(horaCompleta)) return "";
            string[] partes = horaCompleta.Split(' ');
            return partes.Length >= 1 ? partes[0] : ""; // "7:00"
        }

        private string ExtraerHoraFin(string textoHoraFin)
        {
            if (string.IsNullOrEmpty(textoHoraFin) || !textoHoraFin.Contains(":")) return "";
            string[] partes = textoHoraFin.Split(' ');
            return partes.Length >= 3 ? partes[2] : ""; // "7:50"
        }
    }
}