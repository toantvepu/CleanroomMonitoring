// 4. SensorMapViewComponent.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorMap2ViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorMap2ViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int roomId)
        {
            var cleanRoom = await _context.CleanRooms
                .FirstOrDefaultAsync(r => r.RoomID == roomId);

            if (cleanRoom == null) {
                return Content("Phòng không tồn tại");
            }

            // Lấy tất cả vị trí cảm biến trong phòng này
            var sensorLocations = await _context.SensorLocations
                .Include(l => l.SensorInfo)
                .ThenInclude(s => s.SensorType)
                .Where(l => l.SensorInfo.RoomID == roomId && l.IsActive)
                .ToListAsync();

            // Lấy trạng thái kết nối của các cảm biến
            var sensorIds = sensorLocations.Select(l => l.SensorInfoID ?? 0).ToList();
            var connectionStatuses = await _context.SensorConnectionStatuss
                .Where(s => sensorIds.Contains(s.SensorInfoID))
                .ToListAsync();

            // Lấy các cờ cảnh báo của các cảm biến
            var sensorFlags = await _context.SensorFlagss
                .Where(f => sensorIds.Contains(f.SensorInfoID))
                .ToListAsync();

            // Lấy các đọc mới nhất của các cảm biến
            var latestReadings = new Dictionary<int, SensorReading>();
            foreach (var sensorId in sensorIds.Where(id => id > 0)) {
                var latest = await _context.SensorReadings
                    .Where(r => r.SensorInfoID == sensorId)
                    .OrderByDescending(r => r.ReadingTime)
                    .FirstOrDefaultAsync();

                if (latest != null) {
                    latestReadings[sensorId] = latest;
                }
            }

            var viewModel = new SensorMapViewModel {
                CleanRoom = cleanRoom,
                SensorLocations = sensorLocations,
                ConnectionStatuses = connectionStatuses,
                SensorFlags = sensorFlags,
                LatestReadings = latestReadings
            };

            return View(viewModel);
        }
    }

    public class SensorMapViewModel
    {
        public CleanRoom CleanRoom { get; set; }
        public List<SensorLocation> SensorLocations { get; set; }
        public List<SensorConnectionStatus> ConnectionStatuses { get; set; }
        public List<SensorFlags> SensorFlags { get; set; }
        public Dictionary<int, SensorReading> LatestReadings { get; set; }
    }
}