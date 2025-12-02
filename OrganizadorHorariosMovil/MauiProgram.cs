using Microsoft.Extensions.Logging;
using OrganizadorHorariosMovil.Services;
using OrganizadorHorariosMovil.ViewModels;
using OrganizadorHorariosMovil.Views;

namespace OrganizadorHorariosMovil
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registrar servicios
            builder.Services.AddSingleton<IScreenshotService, MauiScreenshotService>();
            builder.Services.AddSingleton<HorarioViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AgregarMateriaPage>();
            builder.Services.AddTransient<VistaPreviaPage>();


            return builder.Build();
        }
    }

}