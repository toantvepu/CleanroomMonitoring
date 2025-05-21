using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class RoomsSummaryViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public RoomsSummaryViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var totalRooms = await _context.CleanRooms.CountAsync();
            var activeRooms = await _context.CleanRooms.CountAsync(r =>
                r.SensorInfos.Any(s => s.IsActive == true));

            var model = new RoomsSummaryViewModel {
                TotalRooms = totalRooms,
                ActiveRooms = activeRooms,
                InactiveRooms = totalRooms - activeRooms
            };

            return View(model);
        }
    }

    public class RoomsSummaryViewModel
    {
        public int TotalRooms { get; set; }
        public int ActiveRooms { get; set; }
        public int InactiveRooms { get; set; }
    }
}