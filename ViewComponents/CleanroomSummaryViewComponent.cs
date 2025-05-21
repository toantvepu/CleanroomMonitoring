using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class CleanroomSummaryViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public CleanroomSummaryViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? factoryId = null, int? count = null)
        {
            var query = _context.CleanRooms
                .Include(r => r.Factory)
                .AsQueryable();

            if (factoryId.HasValue) {
                query = query.Where(r => r.FactoryID == factoryId.Value);
            }
            if (count.HasValue && count.Value > 0) {
                query = query.Take(count.Value);
            }

            var cleanRooms = await query.ToListAsync();
            var viewModel = new CleanroomSummaryViewModel {
                CleanRooms = new List<CleanroomStatusData>()
            };

            foreach (var room in cleanRooms) {
                // Đếm số cảm biến trong phòng
                var sensorCount = await _context.SensorInfos
                    .CountAsync(s => s.RoomID == room.RoomID);

                // Lấy các cảm biến với các loại chính
                var sensors = await _context.SensorInfos
                    .Include(s => s.SensorType)
                    .Where(s => s.RoomID == room.RoomID)
                    .ToListAsync();

                // Lấy số cảm biến đang kết nối
                var connectedSensors = await _context.Set<SensorConnectionStatus>()
                    .CountAsync(s => sensors.Select(si => si.SensorInfoID).Contains(s.SensorInfoID) && s.IsConnected);

                // Lấy số cảnh báo chưa xử lý
                var activeAlerts = await _context.AlertHistorys
                    .CountAsync(a => sensors.Select(si => si.SensorInfoID).Contains(a.SensorInfoID) && !a.IsHandled);

                // Lấy các đọc mới nhất cho các loại cảm biến chính
                var latestReadings = new Dictionary<string, decimal?>();

                var temperatureSensor = sensors.FirstOrDefault(s => s.SensorType.TypeName.Contains("Temperature")
                    || s.SensorType.TypeName.Contains("Nhiệt độ"));
                if (temperatureSensor != null) {
                    var latest = await _context.SensorReadings
                        .Where(r => r.SensorInfoID == temperatureSensor.SensorInfoID)
                        .OrderByDescending(r => r.ReadingTime)
                        .FirstOrDefaultAsync();
                    if (latest != null) {
                        latestReadings["Temperature"] = latest.ReadingValue;
                    }
                }

                var humiditySensor = sensors.FirstOrDefault(s => s.SensorType.TypeName.Contains("Humidity")
                    || s.SensorType.TypeName.Contains("Độ ẩm"));
                if (humiditySensor != null) {
                    var latest = await _context.SensorReadings
                        .Where(r => r.SensorInfoID == humiditySensor.SensorInfoID)
                        .OrderByDescending(r => r.ReadingTime)
                        .FirstOrDefaultAsync();
                    if (latest != null) {
                        latestReadings["Humidity"] = latest.ReadingValue;
                    }
                }

                var pressureSensor = sensors.FirstOrDefault(s => s.SensorType.TypeName.Contains("Pressure")
                    || s.SensorType.TypeName.Contains("Áp suất"));
                if (pressureSensor != null) {
                    var latest = await _context.SensorReadings
                        .Where(r => r.SensorInfoID == pressureSensor.SensorInfoID)
                        .OrderByDescending(r => r.ReadingTime)
                        .FirstOrDefaultAsync();
                    if (latest != null) {
                        latestReadings["Pressure"] = latest.ReadingValue;
                    }
                }

                var roomStatus = new CleanroomStatusData {
                    CleanRoom = room,
                    SensorCount = sensorCount,
                    ConnectedSensors = connectedSensors,
                    DisconnectedSensors = sensorCount - connectedSensors,
                    ActiveAlerts = activeAlerts,
                    LatestReadings = latestReadings
                };

                viewModel.CleanRooms.Add(roomStatus);
            }
            //Xắp xếp
            viewModel.CleanRooms = viewModel.CleanRooms
            .OrderByDescending(r => r.ActiveAlerts > 0)
            .ThenByDescending(r => r.ActiveAlerts)
            .ToList();


            return View(viewModel);
        }
    }

    public class CleanroomSummaryViewModel
    {
        public List<CleanroomStatusData> CleanRooms { get; set; }
    }

    public class CleanroomStatusData
    {
        public CleanRoom CleanRoom { get; set; }
        public int SensorCount { get; set; }
        public int ConnectedSensors { get; set; }
        public int DisconnectedSensors { get; set; }
        public int ActiveAlerts { get; set; }
        public Dictionary<string, decimal?> LatestReadings { get; set; }
    }
}