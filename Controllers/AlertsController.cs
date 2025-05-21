using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.Controllers
{
    public class AlertsController : Controller
    {
        private readonly dbDataContext _context;

        public AlertsController(dbDataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // GET: Alerts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Lấy thông tin AlertHistory
            var alert = await _context.AlertHistorys
                .Include(a => a.SensorInfo)
                    .ThenInclude(s => s.CleanRoom)
                .Include(a => a.SensorInfo)
                    .ThenInclude(s => s.SensorType)
                .Include(a => a.SensorInfo)
                    .ThenInclude(s => s.SensorLocations)
                .FirstOrDefaultAsync(a => a.AlertID == id);

            if (alert == null) {
                return NotFound();
            }

            var sensorInfoId = alert.SensorInfoID;

            // Lấy thông tin bất thường từ SensorFlags
            var sensorFlag = await _context.Set<SensorFlags>()
                .FirstOrDefaultAsync(f => f.SensorInfoID == sensorInfoId);

            // Lấy trạng thái kết nối từ SensorConnectionStatus
            var connectionStatus = await _context.Set<SensorConnectionStatus>()
                .FirstOrDefaultAsync(c => c.SensorInfoID == sensorInfoId);

            // Lấy lịch sử kiểm tra sức khỏe
            var healthChecks = await _context.Set<SensorHealthCheckHistory>()
                .Where(h => h.SensorInfoID == sensorInfoId)
                .OrderByDescending(h => h.CheckTime)
                .ToListAsync();

            // Tạo ViewModel
            var vm = new AlertDetailsViewModel {
                CleanRoomName = alert.SensorInfo?.CleanRoom?.RoomName ?? "",
                SensorName = alert.SensorInfo?.SensorName ?? "",
                SensorTypeName = alert.SensorInfo?.SensorType?.TypeName ?? "",
                LocationName = alert.SensorInfo?.SensorLocations?.FirstOrDefault()?.LocationName ?? "",
                AlertTime = alert.AlertTime ?? DateTime.MinValue,

                // Thông tin cảnh báo bất thường
                HasAbnormalValue = sensorFlag?.HasAbnormalValue ?? false,
                AbnormalValueType = sensorFlag?.AbnormalValueType ?? "",
                AbnormalValueTime = sensorFlag?.AbnormalValueTime,
                AbnormalValueDescription = sensorFlag?.AbnormalValueDescription ?? "",

                // Thông tin kết nối
                IsConnected = connectionStatus?.IsConnected ?? true,
                LastConnectionTime = connectionStatus?.LastConnectionTime,
                DisconnectionCount = connectionStatus?.DisconnectionCount ?? 0,
                LastIssueType = connectionStatus?.LastIssueType ?? "",
                LastIssueDescription = connectionStatus?.LastIssueDescription ?? "",

                // Lịch sử kiểm tra
                HealthCheckHistory = healthChecks
            };

            return View(vm);
        }
    }
    public class AlertDetailsViewModel
    {
        public string CleanRoomName { get; set; }
        public string SensorName { get; set; }
        public string SensorTypeName { get; set; }
        public string LocationName { get; set; }
        public DateTime AlertTime { get; set; }

        // Thông tin cảnh báo bất thường
        public bool HasAbnormalValue { get; set; }
        public string AbnormalValueType { get; set; }
        public DateTime? AbnormalValueTime { get; set; }
        public string AbnormalValueDescription { get; set; }

        // Thông tin kết nối
        public bool IsConnected { get; set; }
        public DateTime? LastConnectionTime { get; set; }
        public int DisconnectionCount { get; set; }
        public string LastIssueType { get; set; }
        public string LastIssueDescription { get; set; }

        // Lịch sử kiểm tra
        public List<SensorHealthCheckHistory> HealthCheckHistory { get; set; }
    }

}
