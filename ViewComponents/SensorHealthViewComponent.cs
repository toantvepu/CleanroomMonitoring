// 5. SensorHealthViewComponent.cs
using System;
using System.Linq;
using System.Threading.Tasks;

using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorHealthViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorHealthViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? roomId = null)
        {
            var sensorQuery = _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .AsQueryable();

            if (roomId.HasValue) {
                sensorQuery = sensorQuery.Where(s => s.RoomID == roomId.Value);
            }

            var sensors = await sensorQuery.ToListAsync();
            var sensorIds = sensors.Select(s => s.SensorInfoID).ToList();

            // Lấy trạng thái kết nối của các cảm biến
            var connectionStatuses = await _context.SensorConnectionStatuss
                .Where(s => sensorIds.Contains(s.SensorInfoID))
                .ToListAsync();

            // Lấy các cờ cảnh báo của các cảm biến
            var sensorFlags = await _context.SensorFlagss
                .Where(f => sensorIds.Contains(f.SensorInfoID))
                .ToListAsync();

            // Lấy lịch sử kiểm tra sức khỏe cảm biến
            var healthCheckHistories = await _context.SensorHealthCheckHistorys
                .Where(h => sensorIds.Contains(h.SensorInfoID))
                .OrderByDescending(h => h.CheckTime)
                .Take(100)  // Giới hạn số lượng để tránh quá tải
                .ToListAsync();

            var viewModel = new SensorHealthViewModel {
                Sensors = sensors,
                ConnectionStatuses = connectionStatuses,
                SensorFlags = sensorFlags,
                HealthCheckHistories = healthCheckHistories
            };

            return View(viewModel);
        }
    }

    public class SensorHealthViewModel
    {
        public List<SensorInfo> Sensors { get; set; }
        public List<SensorConnectionStatus> ConnectionStatuses { get; set; }
        public List<SensorFlags> SensorFlags { get; set; }
        public List<SensorHealthCheckHistory> HealthCheckHistories { get; set; }
    }
}