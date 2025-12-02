using OrganizadorHorariosMovil.ViewModels;
using OrganizadorHorariosMovil.Views;
using OrganizadorHorariosMovil.Services;

namespace OrganizadorHorariosMovil
{
    public partial class MainPage : ContentPage
    {
        private HorarioViewModel _viewModel;
        private readonly IScreenshotService _screenshotService;

        public MainPage(HorarioViewModel viewModel, IScreenshotService screenshotService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _screenshotService = screenshotService;
            BindingContext = _viewModel;

            ConfigurarHorario();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ActualizarHorario();
        }

        private void ConfigurarHorario()
        {
            gridHorario.Children.Clear();
            gridHorario.RowDefinitions.Clear();
            gridHorario.ColumnDefinitions.Clear();

            // Configurar columnas (HORA + 5 días)
            gridHorario.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            for (int i = 0; i < _viewModel.Dias.Length; i++)
            {
                gridHorario.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            // Configurar filas (encabezado + horas)
            gridHorario.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < _viewModel.Horas.Length; i++)
            {
                gridHorario.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Agregar encabezados de columnas
            AgregarCelda(0, 0, "HORA", Colors.Navy, Colors.White, true);
            for (int i = 0; i < _viewModel.Dias.Length; i++)
            {
                AgregarCelda(0, i + 1, _viewModel.Dias[i], Colors.Navy, Colors.White, true);
            }

            // Agregar horas en la primera columna
            for (int i = 0; i < _viewModel.Horas.Length; i++)
            {
                var backgroundColor = _viewModel.Horas[i] == "10:40 a 11:30" ? 
                    Colors.LightYellow : (i % 2 == 0 ? Colors.LightCyan : Colors.White);
                
                AgregarCelda(i + 1, 0, _viewModel.Horas[i], backgroundColor, Colors.Black, false);
            }
        }

        private void ActualizarHorario()
        {
            // Limpiar celdas de materias (mantener este código)
            for (int row = 1; row <= _viewModel.Horas.Length; row++)
            {
                for (int col = 1; col <= _viewModel.Dias.Length; col++)
                {
                    var existingView = gridHorario.Children
                        .FirstOrDefault(c => Grid.GetRow((BindableObject)c) == row && Grid.GetColumn((BindableObject)c) == col);
                    if (existingView != null)
                    {
                        gridHorario.Children.Remove(existingView);
                    }
                }
            }

            // Agregar materias al horario - CÓDIGO CORREGIDO:
            foreach (var materia in _viewModel.Materias)
            {
                int columnaDia = Array.IndexOf(_viewModel.Dias, materia.Dia?.ToUpper()) + 1;

                // Encontrar la fila basada en la hora de inicio
                int filaInicio = -1;
                for (int i = 0; i < _viewModel.Horas.Length; i++)
                {
                    if (_viewModel.Horas[i].StartsWith(materia.HoraInicio))
                    {
                        filaInicio = i + 1;
                        break;
                    }
                }

                if (filaInicio > 0 && columnaDia > 0)
                {
                    // Agregar la materia por la duración especificada
                    for (int i = 0; i < materia.Duracion; i++)
                    {
                        int filaActual = filaInicio + i;
                        if (filaActual <= _viewModel.Horas.Length)
                        {
                            AgregarCeldaMateria(filaActual, columnaDia, materia);
                        }
                    }
                }
                else
                {
                    // Debug: Ver qué está pasando
                    System.Diagnostics.Debug.WriteLine($"No se pudo ubicar materia: {materia.Nombre}");
                    System.Diagnostics.Debug.WriteLine($"Día: {materia.Dia}, HoraInicio: {materia.HoraInicio}");
                    System.Diagnostics.Debug.WriteLine($"Columna encontrada: {columnaDia}, Fila encontrada: {filaInicio}");
                }
            }

            lblContador.Text = $"Materias agregadas: {_viewModel.MateriasCount}";
        }





        private void AgregarCelda(int row, int col, string texto, Color backgroundColor, Color textColor, bool isHeader = false)
        {
            var label = new Label
            {
                Text = texto,
                BackgroundColor = backgroundColor,
                TextColor = textColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = isHeader ? FontAttributes.Bold : FontAttributes.None,
                FontSize = isHeader ? 14 : 12,
                Padding = new Thickness(8),
                LineBreakMode = LineBreakMode.WordWrap
            };

            Grid.SetRow(label, row);
            Grid.SetColumn(label, col);
            gridHorario.Children.Add(label);
        }

        private void AgregarCeldaMateria(int row, int col, Models.Materia materia)
        {
            var border = new Border
            {
                BackgroundColor = ObtenerColorMateria(materia.Nombre),
                Stroke = Colors.LightGray,
                StrokeThickness = 1,
                Padding = new Thickness(5)
            };

            var stackLayout = new VerticalStackLayout();

            stackLayout.Children.Add(new Label
            {
                Text = materia.Nombre,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap
            });

            stackLayout.Children.Add(new Label
            {
                Text = materia.Maestro,
                FontSize = 9,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap
            });

            border.Content = stackLayout;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            gridHorario.Children.Add(border);
        }

        private Color ObtenerColorMateria(string nombreMateria)
        {
            var colores = new[]
            {
                Colors.LightGreen,
                Colors.LightBlue,
                Colors.LightPink,
                Colors.LightYellow,
                Colors.LightCoral,
                Colors.LightSeaGreen,
                Colors.LightSteelBlue
            };

            int hash = Math.Abs(nombreMateria.GetHashCode());
            return colores[hash % colores.Length];
        }

        private async void OnAgregarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AgregarMateriaPage(_viewModel));
        }

        private async void OnLimpiarClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirmar", "¿Estás seguro de que quieres limpiar todo el horario?", "Sí", "No");
            if (answer)
            {
                _viewModel.LimpiarHorario();
                ActualizarHorario();
                await DisplayAlert("Éxito", "Horario limpiado correctamente", "OK");
            }
        }

        private async void OnExportarClicked(object sender, EventArgs e)
        {
            try
            {
                // Mostrar indicador de carga
                await DisplayAlert("Exportando", "Generando imagen del horario...", "OK");

                // Capturar y guardar la imagen
                var filePath = await _screenshotService.CaptureAndSaveAsync(this);

                if (!string.IsNullOrEmpty(filePath))
                {
                    // Mostrar opciones para compartir
                    await ShareImage(filePath);
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo generar la imagen", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al exportar: {ex.Message}", "OK");
            }
        }

        private async Task ShareImage(string filePath)
        {
            var options = new ShareFileRequest
            {
                Title = "Compartir Horario",
                File = new ShareFile(filePath)
            };

            try
            {
                await Share.Default.RequestAsync(options);
            }
            catch (Exception ex)
            {
                // Si no se puede compartir, mostrar mensaje de éxito
                await DisplayAlert("Éxito",
                    $"Horario exportado como imagen en:\n{filePath}",
                    "OK");
            }
        }

        private async void OnVistaPreviaClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Vista Previa", "Funcionalidad de vista previa en desarrollo", "OK");
        }
    }
}