using CleanroomMonitoring.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class CleanRoomDashboardViewComponent : ViewComponent
    {
        private readonly IRoomDataService _roomDataService;

        public CleanRoomDashboardViewComponent(IRoomDataService roomDataService)
        {
            _roomDataService = roomDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int hours = 24)
        {
            var dashboardData = await _roomDataService.GetDashboardDataAsync(hours);
            return View(dashboardData);
        }
    }
}
