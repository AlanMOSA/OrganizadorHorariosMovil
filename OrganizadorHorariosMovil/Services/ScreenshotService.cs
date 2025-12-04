using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using SkiaSharp;

#if ANDROID
using Microsoft.Maui.Platform;
using Android.Views;
#elif IOS
using Microsoft.Maui.Platform;
using UIKit;
#endif

// Alias para evitar ambigüedad con Android.Views.View y con Microsoft.Maui.Platform.ContentView
using MauiView = Microsoft.Maui.Controls.View;
using MauiContentView = Microsoft.Maui.Controls.ContentView;

namespace OrganizadorHorariosMovil.Services
{
    // Implementación revisada que captura sólo el rectángulo del Grid identificado por "gridHorario"
    public class GridScreenshotService : IScreenshotService
    {
        const string GridName = "gridHorario";

        public async Task<string> CaptureAndSaveAsync(ContentPage page)
        {
            try
            {
                if (page == null)
                    throw new ArgumentNullException(nameof(page));

                // Buscar el Grid por name o StyleId
                var grid = FindGridRecursively(page, GridName);
                if (grid == null)
                {
                    await page.DisplayAlert("Información", "No se encontró el grid del horario", "OK");
                    return await CaptureFullPageAsFallback(page);
                }

                if (!((Grid)grid).Children.Any())
                {
                    await page.DisplayAlert("Información", "El horario está vacío", "OK");
                    return null;
                }

                // Asegurar que la vista esté renderizada
                await EnsureRendered(grid);

                if (!Screenshot.Default.IsCaptureSupported)
                {
                    await page.DisplayAlert("Información", "Captura no soportada en este dispositivo", "OK");
                    return await CaptureFullPageAsFallback(page);
                }

                // Capturar pantalla completa
                var screenshot = await Screenshot.Default.CaptureAsync();
                await using var fullStream = await screenshot.OpenReadAsync();
                fullStream.Position = 0;

                // Decodificar con SkiaSharp
                using var skStream = new SKManagedStream(fullStream);
                using var original = SKBitmap.Decode(skStream);
                if (original == null)
                    throw new InvalidOperationException("No se pudo decodificar la imagen completa.");

                // Obtener bounds del grid en píxeles nativos
                var bounds = GetNativeBoundsInPixels(grid, page);

                // Construir rect de recorte y recortar
                var cropRect = new SKRectI(bounds.x, bounds.y, bounds.x + bounds.width, bounds.y + bounds.height);

                // Clamp dentro de la imagen
                cropRect.Left = Math.Max(0, cropRect.Left);
                cropRect.Top = Math.Max(0, cropRect.Top);
                cropRect.Right = Math.Min(original.Width, cropRect.Right);
                cropRect.Bottom = Math.Min(original.Height, cropRect.Bottom);

                var w = cropRect.Width;
                var h = cropRect.Height;
                if (w <= 0 || h <= 0)
                    throw new InvalidOperationException("Rectángulo de recorte inválido o fuera de la imagen.");

                var cropped = new SKBitmap(w, h);
                original.ExtractSubset(cropped, cropRect);

                using var image = SKImage.FromBitmap(cropped);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                // Guardar archivo
                var filename = $"Horario_Grid_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(FileSystem.CacheDirectory, filename);

                using (var fs = File.OpenWrite(filePath))
                {
                    data.SaveTo(fs);
                    await fs.FlushAsync();
                }

                // Compartir
                await ShareImage(filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturando grid: {ex}");
                return await CaptureFullPageAsFallback(page);
            }
        }

        // Versión que devuelve Stream de la imagen recortada
        public async Task<Stream> CaptureAsync(ContentPage page)
        {
            try
            {
                if (page == null)
                    throw new ArgumentNullException(nameof(page));

                var grid = FindGridRecursively(page, GridName);
                if (grid == null || !Screenshot.Default.IsCaptureSupported)
                {
                    var screenshot = await Screenshot.Default.CaptureAsync();
                    return await screenshot.OpenReadAsync();
                }

                await EnsureRendered(grid);

                var screenshotFull = await Screenshot.Default.CaptureAsync();
                await using var fullStream = await screenshotFull.OpenReadAsync();
                fullStream.Position = 0;

                using var skStream = new SKManagedStream(fullStream);
                using var original = SKBitmap.Decode(skStream);
                if (original == null)
                {
                    fullStream.Position = 0;
                    var ms = new MemoryStream();
                    await fullStream.CopyToAsync(ms);
                    ms.Position = 0;
                    return ms;
                }

                var bounds = GetNativeBoundsInPixels(grid, page);
                var cropRect = new SKRectI(bounds.x, bounds.y, bounds.x + bounds.width, bounds.y + bounds.height);

                cropRect.Left = Math.Max(0, cropRect.Left);
                cropRect.Top = Math.Max(0, cropRect.Top);
                cropRect.Right = Math.Min(original.Width, cropRect.Right);
                cropRect.Bottom = Math.Min(original.Height, cropRect.Bottom);

                var w = cropRect.Width;
                var h = cropRect.Height;
                if (w <= 0 || h <= 0)
                {
                    fullStream.Position = 0;
                    var ms = new MemoryStream();
                    await fullStream.CopyToAsync(ms);
                    ms.Position = 0;
                    return ms;
                }

                var cropped = new SKBitmap(w, h);
                original.ExtractSubset(cropped, cropRect);
                using var image = SKImage.FromBitmap(cropped);
                using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);

                var outMs = new MemoryStream();
                encoded.SaveTo(outMs);
                outMs.Position = 0;
                return outMs;
            }
            catch
            {
                // fallback to full screenshot
                if (Screenshot.Default.IsCaptureSupported)
                {
                    var s = await Screenshot.Default.CaptureAsync();
                    return await s.OpenReadAsync();
                }

                return new MemoryStream();
            }
        }

        private MauiView FindGridRecursively(VisualElement parent, string gridName)
        {
            try
            {
                if (parent is ContentPage page)
                {
                    // Try by x:Name first
                    var named = page.FindByName<MauiView>(gridName);
                    if (named is MauiView v) return v;

                    // then StyleId fallback
                    return FindGridRecursively(page.Content, gridName);
                }

                if (parent is Layout layout)
                {
                    foreach (var child in layout.Children)
                    {
                        if (child is VisualElement visualChild)
                        {
                            // StyleId (usado en tu implementación previa)
                            if (visualChild.StyleId == gridName && visualChild is MauiView vf)
                                return vf;

                            var found = FindGridRecursively(visualChild, gridName);
                            if (found != null)
                                return found;
                        }
                    }
                }
                else if (parent is MauiContentView contentView && contentView.Content != null)
                {
                    return FindGridRecursively(contentView.Content, gridName);
                }
                else if (parent is ScrollView scroll && scroll.Content != null)
                {
                    return FindGridRecursively(scroll.Content, gridName);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task EnsureRendered(MauiView view)
        {
            // Esperar hasta que handler exista y la vista tenga tamaño
            int tries = 0;
            while ((view?.Handler == null || view.Width <= 0 || view.Height <= 0) && tries < 10)
            {
                await Task.Delay(100);
                tries++;
            }

            // Pequeño retardo adicional para garantizar renderizado nativo
            await Task.Delay(120);
        }

        // Devuelve (x,y,width,height) en pixeles nativos relativos a la imagen de screenshot (pantalla completa)
        private (int x, int y, int width, int height) GetNativeBoundsInPixels(MauiView view, ContentPage page)
        {
#if ANDROID
            var mauiContext = view.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext no disponible.");
            var nativeView = view.ToPlatform(mauiContext) as Android.Views.View;
            if (nativeView == null)
                throw new InvalidOperationException("No se pudo obtener la vista nativa Android.");

            int[] loc = new int[2];
            nativeView.GetLocationOnScreen(loc);
            int x = loc[0];
            int y = loc[1];
            int width = nativeView.Width;
            int height = nativeView.Height;

            // Android GetLocationOnScreen returns pixels que coinciden con la captura
            return (x, y, width, height);
#elif IOS
            var mauiContext = view.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext no disponible.");
            var nativeView = view.ToPlatform(mauiContext) as UIKit.UIView;
            if (nativeView == null)
                throw new InvalidOperationException("No se pudo obtener la vista nativa iOS.");

            var window = nativeView.Window ?? UIApplication.SharedApplication.KeyWindow;
            var frameInWindow = nativeView.ConvertRectToView(nativeView.Bounds, window);
            var scale = (float)UIScreen.MainScreen.Scale;

            int x = (int)Math.Round(frameInWindow.X * scale);
            int y = (int)Math.Round(frameInWindow.Y * scale);
            int width = (int)Math.Round(frameInWindow.Width * scale);
            int height = (int)Math.Round(frameInWindow.Height * scale);

            return (x, y, width, height);
#else
            // Fallback multiplataforma
            var density = DeviceDisplay.MainDisplayInfo.Density;
            double xD = view.Bounds.X;
            double yD = view.Bounds.Y;

            VisualElement parent = view.Parent as VisualElement;
            while (parent != null && parent != page)
            {
                xD += parent.Bounds.X;
                yD += parent.Bounds.Y;
                parent = parent.Parent as VisualElement;
            }

            int x = (int)Math.Round(xD * density);
            int y = (int)Math.Round(yD * density);
            int width = (int)Math.Round(view.Width * density);
            int height = (int)Math.Round(view.Height * density);

            return (x, y, width, height);
#endif
        }

        private async Task<string> CaptureFullPageAsFallback(ContentPage page)
        {
            try
            {
                await page.DisplayAlert("Información", "Capturando toda la página como alternativa...", "OK");

                if (Screenshot.Default.IsCaptureSupported)
                {
                    var screenshot = await Screenshot.Default.CaptureAsync();
                    var stream = await screenshot.OpenReadAsync();
                    var filePath = await SaveImageToFile(stream, "Horario_Completo");

                    await ShareImage(filePath);
                    return filePath;
                }

                return null;
            }
            catch
            {
                await ShareAsTextFallback();
                return null;
            }
        }

        private async Task<string> SaveImageToFile(Stream imageStream, string prefix)
        {
            string fileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (FileStream fileStream = File.Create(filePath))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            return filePath;
        }

        private async Task ShareImage(string filePath)
        {
            if (File.Exists(filePath))
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "📅 Mi Horario Universitario",
                    File = new ShareFile(filePath),
                    PresentationSourceBounds = new Microsoft.Maui.Graphics.Rect(0, 0, 600, 800)
                });
            }
        }

        private async Task ShareAsTextFallback()
        {
            string horarioText = "📅 HORARIO UNIVERSITARIO\n\n";
            horarioText += $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n";
            horarioText += "Exportado desde: Organizador de Horarios App\n";
            horarioText += "✨ ¡Próximamente: Captura del grid específico!";

            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Mi Horario",
                Text = horarioText
            });
        }
    }
}