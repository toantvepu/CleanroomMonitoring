using Microsoft.AspNetCore.Mvc;

namespace CleanroomMonitoring.Web.Controllers
{
    public class ComponentsController : Controller
    {
        public IActionResult SensorReadingsChart(string range = "24h", int interval = 15, string area = "ALL")
        {
            var result = ViewComponent("SensorReadingsChart", new { range, interval, area });
            return result;
        }
        
        [Route("Components/[action]")]
        public IActionResult GetSensorMap(string area = "1F", string sensorType = "all")
        {
            return ViewComponent("EnhancedSensorMap", new { area = area, sensorType = sensorType });
        }
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
