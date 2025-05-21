using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Services;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly dbDataContext _context;
        private readonly ISensorDataService _sensorDataService;

        public AnalyticsController(dbDataContext context, ISensorDataService sensorDataService)
        {
            _context = context;
            _sensorDataService = sensorDataService;
        }

        public async Task<IActionResult> Index(int? SelectedRoomId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var viewModel = new AnalyticsViewModel();

            // Get all clean rooms
            viewModel.CleanRooms = await _context.CleanRooms
                .Select(r => new CleanRoomViewModel {
                    RoomID = r.RoomID,
                    RoomName = r.RoomName,
                    CleanRoomClass = r.CleanRoomClass
                })
                .ToListAsync();

            // Set default date range (last 7 days)
            viewModel.StartDate = startDate ?? DateTime.Now.AddDays(-1);
            viewModel.EndDate = endDate ?? DateTime.Now;

            // Get data for selected room
            if (SelectedRoomId.HasValue) {
                viewModel.SelectedRoomId = SelectedRoomId.Value;

                var room = await _context.CleanRooms
                    .FirstOrDefaultAsync(r => r.RoomID == SelectedRoomId.Value);

                if (room != null) {
                    viewModel.SelectedRoomName = room.RoomName;

                    // Get sensors for this room
                    var sensors = await _context.SensorInfos
                        .Include(s => s.SensorType)
                        .Where(s => s.RoomID == SelectedRoomId.Value)
                        .ToListAsync();

                    viewModel.Sensors = sensors.Select(s => new SensorViewModel {
                        SensorInfoID = s.SensorInfoID,
                        SensorName = s.SensorName,
                        SensorTypeName = s.SensorType.TypeName,
                        Unit = s.SensorType.Unit
                    }).ToList();

                    // Get aggregated statistics for each sensor
                    foreach (var sensor in viewModel.Sensors) {
                        var stats = await _sensorDataService.GetSensorStatisticsAsync(
                            sensor.SensorInfoID,
                            viewModel.StartDate,
                            viewModel.EndDate);

                        viewModel.SensorStatistics[sensor.SensorInfoID] = stats;
                    }

                    // Get alert events
                    var alertEvents = await _sensorDataService.GetRoomAlertsAsync(
                        SelectedRoomId.Value,
                        viewModel.StartDate,
                        viewModel.EndDate);

                    viewModel.AlertEvents = alertEvents;

                    // Calculate uptime percentage
                    decimal uptimePercentage = 0;
                    if (alertEvents.Any()) {
                        TimeSpan totalDuration = viewModel.EndDate - viewModel.StartDate;
                        TimeSpan totalAlertTime = TimeSpan.Zero;

                        foreach (var sensorAlerts in alertEvents.Values) {
                            foreach (var alert in sensorAlerts) {
                                totalAlertTime += alert.Duration;
                            }
                        }

                        double alertTimeHours = totalAlertTime.TotalHours;
                        double totalTimeHours = totalDuration.TotalHours;

                        uptimePercentage = (decimal)(100 - (alertTimeHours / totalTimeHours * 100));
                    }
                    else {
                        uptimePercentage = 100;
                    }

                    viewModel.UptimePercentage = Math.Round(uptimePercentage, 2);

                    // Get daily averages for historical trending
                    foreach (var sensor in viewModel.Sensors) {
                        var dailyAverages = await _sensorDataService.GetDailyAveragesAsync(
                            sensor.SensorInfoID,
                            viewModel.StartDate,
                            viewModel.EndDate);

                        viewModel.DailyAverages[sensor.SensorInfoID.ToString()] = dailyAverages;
                    }
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCSV(int roomId, DateTime startDate, DateTime endDate)
        {
            var room = await _context.CleanRooms
                .FirstOrDefaultAsync(r => r.RoomID == roomId);

            if (room == null) {
                return NotFound();
            }

            var sensors = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Where(s => s.RoomID == roomId)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();

            // Header
            csv.AppendLine("SensorID,SensorName,SensorType,Timestamp,Value,IsValid");

            // Data
            foreach (var sensor in sensors) {
                var readings = await _context.SensorReadings
                    .Where(sr => sr.SensorInfoID == sensor.SensorInfoID &&
                           sr.ReadingTime >= startDate &&
                           sr.ReadingTime <= endDate)
                    .OrderBy(sr => sr.ReadingTime)
                    .ToListAsync();

                foreach (var reading in readings) {
                    csv.AppendLine($"{sensor.SensorInfoID},{sensor.SensorName},{sensor.SensorType.TypeName},{reading.ReadingTime:yyyy-MM-dd HH:mm:ss},{reading.ReadingValue},{reading.IsValid}");
                }
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
                         "text/csv",
                         $"CleanRoom_{room.RoomName}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> SensorDetail(int sensorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.AddDays(-7);
            var end = endDate ?? DateTime.Now;

            var sensor = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .FirstOrDefaultAsync(s => s.SensorInfoID == sensorId);

            if (sensor == null) {
                return NotFound();
            }

            // Sử dụng service để lấy dữ liệu
            var chartData = await _sensorDataService.GetSensorReadingsDataPointAsync(sensorId, start, end);
            var statistics = await _sensorDataService.GetSensorStatisticsAsync(sensorId, start, end);

            var viewModel = new SensorDetailViewModel {
                SensorInfoID = sensor.SensorInfoID,
                SensorName = sensor.SensorName,
                SensorTypeName = sensor.SensorType.TypeName,
                RoomName = sensor.CleanRoom.RoomName,
                Unit = sensor.SensorType.Unit,
                StartDate = start,
                EndDate = end,
                ChartData = chartData,
                MinValue = statistics.MinValue,
                MaxValue = statistics.MaxValue,
                AvgValue = statistics.AvgValue,
                ValidReadings = statistics.ValidReadings,
                InvalidReadings = statistics.InvalidReadings
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetHourlyData(int sensorId, DateTime startDate, DateTime endDate)
        {
            try {
                var hourlyData = await _sensorDataService.GetHourlyAveragesAsync(sensorId, startDate, endDate);
                return Json(hourlyData);
            }
            catch (Exception ex) {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportSensorData(int sensorId, DateTime startDate, DateTime endDate)
        {
            var sensor = await _context.SensorInfos
                .Include(s => s.SensorType)
                .FirstOrDefaultAsync(s => s.SensorInfoID == sensorId);

            if (sensor == null) {
                return NotFound();
            }

            var readings = await _context.SensorReadings
                .Where(sr => sr.SensorInfoID == sensorId &&
                       sr.ReadingTime >= startDate &&
                       sr.ReadingTime <= endDate)
                .OrderBy(sr => sr.ReadingTime)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();

            // Header
            csv.AppendLine($"SensorID,SensorName,SensorType,Timestamp,Value ({sensor.SensorType.Unit}),IsValid");

            // Data
            foreach (var reading in readings) {
                csv.AppendLine($"{sensor.SensorInfoID},{sensor.SensorName},{sensor.SensorType.TypeName},{reading.ReadingTime:yyyy-MM-dd HH:mm:ss},{reading.ReadingValue},{reading.IsValid}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
                        "text/csv",
                        $"Sensor_{sensor.SensorName}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
        }
    }
}