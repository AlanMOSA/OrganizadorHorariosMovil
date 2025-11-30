//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Maui.Controls.PlatformConfiguration;
//using OrganizadorHorariosMovil.ViewModels;



//namespace OrganizadorHorariosMovil.Services
//{
//    public class ExportService
//    {
//        private readonly HorarioViewModel _viewModel;

//        public ExportService(HorarioViewModel viewModel)
//        {
//            _viewModel = viewModel;
//        }

//        public async Task<bool> ExportarHorarioComoImagen()
//        {
//            try
//            {
//                // Crear un grid similar al de la pantalla principal
//                var gridHorario = CrearGridParaExportacion();

//                // Convertir el grid a imagen
//                var imagen = await ConvertirGridAImagen(gridHorario);

//                // Guardar la imagen
//                return await GuardarImagen(imagen);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error al exportar: {ex.Message}");
//                return false;
//            }
//        }

//        private Grid CrearGridParaExportacion()
//        {
//            var grid = new Grid
//            {
//                ColumnSpacing = 1,
//                RowSpacing = 1,
//                BackgroundColor = Colors.White,
//                Padding = new Thickness(10)
//            };

//            // Configurar columnas (HORA + 5 días)
//            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 120 });
//            for (int i = 0; i < _viewModel.Dias.Length; i++)
//            {
//                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 200 });
//            }

//            // Configurar filas (encabezado + horas)
//            grid.RowDefinitions.Add(new RowDefinition { Height = 40 });
//            for (int i = 0; i < _viewModel.Horas.Length; i++)
//            {
//                grid.RowDefinitions.Add(new RowDefinition { Height = 80 });
//            }

//            // Agregar encabezados
//            AgregarCeldaExportacion(grid, 0, 0, "HORA", Colors.Navy, Colors.White, true);
//            for (int i = 0; i < _viewModel.Dias.Length; i++)
//            {
//                AgregarCeldaExportacion(grid, 0, i + 1, _viewModel.Dias[i], Colors.Navy, Colors.White, true);
//            }

//            // Agregar horas
//            for (int i = 0; i < _viewModel.Horas.Length; i++)
//            {
//                var backgroundColor = _viewModel.Horas[i] == "10:40 a 11:30" ?
//                    Colors.LightYellow : (i % 2 == 0 ? Colors.LightCyan : Colors.White);

//                AgregarCeldaExportacion(grid, i + 1, 0, _viewModel.Horas[i], backgroundColor, Colors.Black, false);
//            }

//            // Agregar materias
//            foreach (var materia in _viewModel.Materias)
//            {
//                int columnaDia = Array.IndexOf(_viewModel.Dias, materia.Dia) + 1;
//                int filaInicio = -1;

//                for (int i = 0; i < _viewModel.Horas.Length; i++)
//                {
//                    if (CoincideHora(_viewModel.Horas[i], materia.HoraInicio, materia.HoraFin))
//                    {
//                        filaInicio = i + 1;
//                        break;
//                    }
//                }

//                if (filaInicio > 0 && columnaDia > 0)
//                {
//                    for (int i = 0; i < materia.Duracion; i++)
//                    {
//                        int filaActual = filaInicio + i;
//                        if (filaActual <= _viewModel.Horas.Length)
//                        {
//                            AgregarCeldaMateriaExportacion(grid, filaActual, columnaDia, materia);
//                        }
//                    }
//                }
//            }

//            return grid;
//        }

//        private void AgregarCeldaExportacion(Grid grid, int row, int col, string texto, Color backgroundColor, Color textColor, bool isHeader)
//        {
//            var border = new Border
//            {
//                BackgroundColor = backgroundColor,
//                Stroke = Colors.LightGray,
//                StrokeThickness = 1,
//                Padding = new Thickness(8),
//                Content = new Label
//                {
//                    Text = texto,
//                    TextColor = textColor,
//                    HorizontalOptions = LayoutOptions.Center,
//                    VerticalOptions = LayoutOptions.Center,
//                    HorizontalTextAlignment = TextAlignment.Center,
//                    VerticalTextAlignment = TextAlignment.Center,
//                    FontAttributes = isHeader ? FontAttributes.Bold : FontAttributes.None,
//                    FontSize = isHeader ? 14 : 12,
//                    LineBreakMode = LineBreakMode.WordWrap
//                }
//            };

//            Grid.SetRow(border, row);
//            Grid.SetColumn(border, col);
//            grid.Children.Add(border);
//        }

//        private void AgregarCeldaMateriaExportacion(Grid grid, int row, int col, Models.Materia materia)
//        {
//            var border = new Border
//            {
//                BackgroundColor = ObtenerColorMateria(materia.Nombre),
//                Stroke = Colors.LightGray,
//                StrokeThickness = 1,
//                Padding = new Thickness(5),
//                Content = new VerticalStackLayout
//                {
//                    Spacing = 2,
//                    Children = {
//                        new Label {
//                            Text = materia.Nombre,
//                            FontSize = 10,
//                            FontAttributes = FontAttributes.Bold,
//                            HorizontalTextAlignment = TextAlignment.Center,
//                            LineBreakMode = LineBreakMode.WordWrap
//                        },
//                        new Label {
//                            Text = materia.Maestro,
//                            FontSize = 9,
//                            HorizontalTextAlignment = TextAlignment.Center,
//                            LineBreakMode = LineBreakMode.WordWrap
//                        }
//                    }
//                }
//            };

//            Grid.SetRow(border, row);
//            Grid.SetColumn(border, col);
//            grid.Children.Add(border);
//        }

//        private async Task<Stream> ConvertirGridAImagen(Grid grid)
//        {
//            // Para MAUI, necesitamos una solución alternativa ya que no hay soporte directo
//            // Vamos a crear una imagen manualmente usando SkiaSharp

//            await Task.Delay(100); // Pequeña pausa para que el grid se renderice

//            // Calculamos el tamaño total de la imagen
//            int width = 1400;
//            int height = 1000;

//            // Creamos un bitmap
//            var bitmap = new SkiaSharp.SKBitmap(width, height);
//            var canvas = new SkiaSharp.SKCanvas(bitmap);

//            // Fondo blanco
//            canvas.Clear(SkiaSharp.SKColors.White);

//            // Dibujar título
//            using var paint = new SkiaSharp.SKPaint
//            {
//                Color = SkiaSharp.SKColors.DarkBlue,
//                TextSize = 24,
//                IsAntialias = true,
//                TextAlign = SkiaSharp.SKTextAlign.Center,
//                Typeface = SkiaSharp.SKTypeface.FromFamilyName("Arial", SkiaSharp.SKFontStyle.Bold)
//            };

//            canvas.DrawText("HORARIO UNIVERSITARIO", width / 2, 40, paint);

//            // Dibujar fecha
//            paint.TextSize = 14;
//            paint.Color = SkiaSharp.SKColors.Gray;
//            canvas.DrawText($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", 20, 70, paint);

//            // Por ahora retornamos un stream vacío - en una implementación real
//            // necesitaríamos dibujar todo el grid manualmente con SkiaSharp
//            var stream = new MemoryStream();
//            // bitmap.Encode(stream, SkiaSharp.SKEncodedImageFormat.Png, 100);

//            return stream;
//        }

//        private async Task<bool> GuardarImagen(Stream imagenStream)
//        {
//            try
//            {
//                var fileName = $"Horario_Universidad_{DateTime.Now:yyyyMMdd_HHmmss}.png";

//                // Para Android
//                if (DeviceInfo.Platform == DevicePlatform.Android)
//                {
//                    var filePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);

//                    using var fileStream = new FileStream(filePath, FileMode.Create);
//                    await imagenStream.CopyToAsync(fileStream);

//                    return true;
//                }
//                // Para Windows
//                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
//                {
//                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

//                    using var fileStream = new FileStream(filePath, FileMode.Create);
//                    await imagenStream.CopyToAsync(fileStream);

//                    return true;
//                }

//                return false;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error al guardar imagen: {ex.Message}");
//                return false;
//            }
//        }

//        private bool CoincideHora(string horaEnGrid, string horaInicioMateria, string horaFinMateria)
//        {
//            try
//            {
//                string[] partesGrid = horaEnGrid.Split(' ');
//                if (partesGrid.Length >= 3)
//                {
//                    string inicioGrid = partesGrid[0];
//                    string finGrid = partesGrid[2];
//                    return inicioGrid == horaInicioMateria && finGrid == horaFinMateria;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error al comparar horas: {ex.Message}");
//            }

//            return false;
//        }

//        private Color ObtenerColorMateria(string nombreMateria)
//        {
//            var colores = new[]
//            {
//                Colors.LightGreen,
//                Colors.LightBlue,
//                Colors.LightPink,
//                Colors.LightYellow,
//                Colors.LightCoral,
//                Colors.LightSeaGreen,
//                Colors.LightSteelBlue
//            };

//            int hash = Math.Abs(nombreMateria.GetHashCode());
//            return colores[hash % colores.Length];
//        }
//    }
//}