using Microsoft.AspNetCore.Mvc;

namespace CleanroomMonitoring.Web.Controllers
{
    public class SensorPerformanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
