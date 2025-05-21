using Microsoft.AspNetCore.Mvc;

namespace CleanroomMonitoring.Web.Controllers
{
    public class SensorStatusController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
