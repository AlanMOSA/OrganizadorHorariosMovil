using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace OrganizadorHorariosMovil.Services
{
    public class MauiScreenshotService : IScreenshotService
    {
        public async Task<string> CaptureAndSaveAsync(ContentPage page)
        {
            try
            {
                // 1. Tomar screenshot usando la API de MAUI
                if (Screenshot.Default.IsCaptureSupported)
                {
                    // Método 1: Capturar la pantalla actual
                    IScreenshotResult screenshot = await Screenshot.Default.CaptureAsync();

                    // Método 2: O capturar un elemento específico (si tu página tiene un Grid con nombre)
                    // var grid = page.FindByName<Grid>("gridHorario");
                    // IScreenshotResult screenshot = await grid.CaptureAsync();

                    // 2. Convertir a stream
                    Stream stream = await screenshot.OpenReadAsync();

                    // 3. Guardar en archivo
                    string filePath = await SaveScreenshotToFile(stream);

                    // 4. Compartir el archivo
                    await ShareScreenshot(filePath);

                    return filePath;
                }
                else
                {
                    // Fallback: compartir como texto
                    await ShareAsText();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Fallback a texto
                await ShareAsText();
                return null;
            }
        }

        private async Task<string> SaveScreenshotToFile(Stream screenshotStream)
        {
            string fileName = $"Horario_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (FileStream fileStream = File.Create(filePath))
            {
                await screenshotStream.CopyToAsync(fileStream);
            }

            screenshotStream.Position = 0;
            return filePath;
        }

        private async Task ShareScreenshot(string filePath)
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Mi Horario Universitario",
                File = new ShareFile(filePath)
            });
        }

        private async Task ShareAsText()
        {
            // Generar texto simple del horario
            string horarioText = "📅 Mi Horario Universitario\n\n";
            horarioText += $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}\n";
            horarioText += "Materias: [Tu lista de materias aquí]\n\n";
            horarioText += "App: Organizador de Horarios";

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Mi Horario",
                Text = horarioText
            });
        }

        // Si tu interfaz requiere este método:
        public async Task<Stream> CaptureAsync(ContentPage page)
        {
            if (Screenshot.Default.IsCaptureSupported)
            {
                var screenshot = await Screenshot.Default.CaptureAsync();
                return await screenshot.OpenReadAsync();
            }

            // Fallback: stream vacío
            return new MemoryStream();
        }
    }
}