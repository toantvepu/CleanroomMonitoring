using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorsSummaryViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorsSummaryViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new SensorsSummaryViewModel {
                TotalSensors = await _context.SensorInfos.CountAsync(),
                ActiveSensors = await _context.SensorInfos.CountAsync(s => s.IsActive == true),
                TemperatureSensors = await _context.SensorInfos.CountAsync(s => s.SensorType.TypeName.Contains("Temperature")),
                HumiditySensors = await _context.SensorInfos.CountAsync(s => s.SensorType.TypeName.Contains("Humidity")),
                PressureSensors = await _context.SensorInfos.CountAsync(s => s.SensorType.TypeName.Contains("Pressure"))
            };

            return View(model);
        }
    }

    public class SensorsSummaryViewModel
    {
        public int TotalSensors { get; set; }
        public int ActiveSensors { get; set; }
        public int TemperatureSensors { get; set; }
        public int HumiditySensors { get; set; }
        public int PressureSensors { get; set; }
    }
}