using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizadorHorariosMovil.Services
{
    public interface IScreenshotService
    {
        Task<string> CaptureAndSaveAsync(ContentPage page);
        //Task<Stream> CaptureAsync(ContentPage page);
    }
}
