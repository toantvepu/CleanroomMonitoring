
using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.Controllers
{
    public class CleanRoomsController : Controller
    {
        private readonly dbDataContext _context;

        public CleanRoomsController(dbDataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? roomId = null)
        {
            var viewModel = new DashboardViewModel();

            // Get all clean rooms with sensor count
            viewModel.CleanRooms = await _context.CleanRooms
                .Select(r => new CleanRoomViewModel {
                    RoomID = r.RoomID,
                    RoomName = r.RoomName,
                    CleanRoomClass = r.CleanRoomClass,
                    SensorCount = _context.SensorInfos.Count(s => s.RoomID == r.RoomID),
                    // Check if any sensors in this room have alerts
                    HasAlerts = _context.SensorInfos
                        .Where(s => s.RoomID == r.RoomID)
                        .Any(s => s.SensorReadings
                            .OrderByDescending(sr => sr.ReadingTime)
                            .FirstOrDefault().IsValid == false)
                })
                .ToListAsync();

            // Get current room details or default to the first room
            int selectedRoomId = roomId ?? viewModel.CleanRooms.FirstOrDefault()?.RoomID ?? 0;

            if (selectedRoomId > 0) {
                // Get detailed room information
                var room = await _context.CleanRooms
                    .Where(r => r.RoomID == selectedRoomId)
                    .FirstOrDefaultAsync();

                if (room != null) {
                    viewModel.CurrentRoom = new CleanRoomDetailsViewModel {
                        RoomID = room.RoomID,
                        RoomName = room.RoomName,
                        CleanRoomClass = room.CleanRoomClass,
                        Area = room.Area,
                        Description = room.Description
                    };

                    // Get sensors for this room
                    viewModel.CurrentRoom.Sensors = await _context.SensorInfos
                        .Where(s => s.RoomID == selectedRoomId)
                        .Select(s => new SensorViewModel {
                            SensorInfoID = s.SensorInfoID,
                            SensorName = s.SensorName,
                            SensorTypeName = s.SensorType.TypeName,
                            Unit = s.SensorType.Unit,
                            IsActive = s.IsActive,
                            LastReading = s.SensorReadings
                                .OrderByDescending(sr => sr.ReadingTime)
                                .FirstOrDefault().ReadingValue,
                            LastReadingTime = s.SensorReadings
                                .OrderByDescending(sr => sr.ReadingTime)
                                .FirstOrDefault().ReadingTime,
                            IsInAlertState = s.SensorReadings
                                .OrderByDescending(sr => sr.ReadingTime)
                                .FirstOrDefault().IsValid == false
                        })
                        .ToListAsync();

                    // Get chart data for each sensor
                    foreach (var sensor in viewModel.CurrentRoom.Sensors) {
                        // Get readings for the last 24 hours
                        var sensorReadings = await _context.SensorReadings
                            .Where(sr => sr.SensorInfoID == sensor.SensorInfoID &&
                                   sr.ReadingTime >= DateTime.Now.AddHours(-24))
                            .OrderBy(sr => sr.ReadingTime)
                            .Select(sr => new ChartDataPoint {
                                Timestamp = sr.ReadingTime ?? DateTime.Now,
                                Value = sr.ReadingValue ?? 0
                            })
                            .ToListAsync();

                        viewModel.ChartData[sensor.SensorInfoID.ToString()] = sensorReadings;

                        // Calculate statistics for this sensor
                        var readings = await _context.SensorReadings
                            .Where(sr => sr.SensorInfoID == sensor.SensorInfoID &&
                                   sr.ReadingTime >= DateTime.Now.AddHours(-24))
                            .Select(sr => sr.ReadingValue ?? 0)
                            .ToListAsync();

                        var status = "Normal";
                        if (sensor.IsInAlertState) {
                            status = "Alert";
                        }
                        else if (readings.Any() &&
                                (readings.Max() > 0.9m * CalculateThreshold(sensor.SensorTypeName) ||
                                 readings.Min() < 0.1m * CalculateThreshold(sensor.SensorTypeName))) {
                            status = "Warning";
                        }

                        viewModel.SensorStatuses[sensor.SensorInfoID] = new SensorStatusViewModel {
                            SensorInfoID = sensor.SensorInfoID,
                            CurrentValue = sensor.LastReading,
                            MinValue = readings.Any() ? readings.Min() : 0,
                            MaxValue = readings.Any() ? readings.Max() : 0,
                            AverageValue = readings.Any() ? Math.Round(readings.Average(), 1) : 0,
                            Unit = sensor.Unit,
                            Status = status
                        };
                    }
                }
            }

            return View(viewModel);
        }

        // This method would be replaced with your actual business logic for thresholds
        private decimal CalculateThreshold(string sensorType)
        {
            // Example logic - in a real system, you would get this from configuration or business rules
            switch (sensorType) {
                case "Temperature": return 25.0m;
                case "Humidity": return 60.0m;
                case "Pressure": return 100.0m;
                case "Particle Count": return 1000.0m;
                default: return 100.0m;
            }
        }

        // API endpoint to get the latest readings for real-time updates
        [HttpGet]
        public async Task<IActionResult> GetLatestReadings(int roomId)
        {
            var latestData = await _context.SensorInfos
                .Where(s => s.RoomID == roomId)
                .Select(s => new {
                    s.SensorInfoID,
                    LatestReading = s.SensorReadings
                        .OrderByDescending(sr => sr.ReadingTime)
                        .FirstOrDefault(),
                    s.SensorType.TypeName,
                    s.SensorType.Unit
                })
                .ToListAsync();

            var result = latestData.Select(d => new {
                SensorID = d.SensorInfoID,
                Value = d.LatestReading?.ReadingValue,
                Timestamp = d.LatestReading?.ReadingTime,
                IsValid = d.LatestReading?.IsValid,
                TypeName = d.TypeName,
                Unit = d.Unit
            });

            return Json(result);
        }


        public IActionResult Details(int id)
        {
            // Logic để lấy thông tin chi tiết phòng
            return View(id);
        }
    }
}
