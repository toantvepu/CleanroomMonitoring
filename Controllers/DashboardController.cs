using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly dbDataContext _context;

        public DashboardController(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? factoryId = null, int? roomId = null)
        {
            ViewData["FactoryId"] = factoryId;
            ViewData["RoomId"] = roomId;

            // Lấy danh sách nhà máy cho dropdown
            ViewData["Factories"] = await _context.Factories
                .OrderBy(f => f.FactoryName)
                .ToListAsync();

            // Lấy danh sách phòng cho dropdown (theo nhà máy nếu được chọn)
            var roomsQuery = _context.CleanRooms.AsQueryable();
            if (factoryId.HasValue) {
                roomsQuery = roomsQuery.Where(r => r.FactoryID == factoryId.Value);
            }
            ViewData["Rooms"] = await roomsQuery
                .OrderBy(r => r.RoomName)
                .ToListAsync();
            // Truyền danh sách phòng để hiển thị trong dropdown
            ViewBag.CleanRooms = await _context.CleanRooms.ToListAsync();
            return View();
        }

        public async Task<IActionResult> RoomDetail(int id)
        {
            var room = await _context.CleanRooms
                .Include(r => r.Factory)
                .FirstOrDefaultAsync(r => r.RoomID == id);

            if (room == null) {
                return NotFound();
            }

            // Lấy danh sách sensor trong phòng này
            var sensors = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Where(s => s.RoomID == id)
                .ToListAsync();

            ViewData["Room"] = room;
            ViewData["Sensors"] = sensors;

            return View();
        }

    }
}