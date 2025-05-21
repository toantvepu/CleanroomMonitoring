using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanroomMonitoring.Web.ViewModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CleanroomMonitoring.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataApiController : ControllerBase
    {
        private readonly dbDataContext _context;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SensorDataApiController(dbDataContext context)
        {
            _context = context;
        }

        [HttpGet("realtime")]
        public async Task<ActionResult<RealTimeData>> GetRealTimeData(int roomId = 0)
        {
            // Determine which sensors to monitor
            var sensorsQuery = _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .Where(s => s.IsActive);

            // Filter by room if specified
            if (roomId > 0) {
                sensorsQuery = sensorsQuery.Where(s => s.RoomID == roomId);
            }

            // Get sensor information first
            var sensorInfos = await sensorsQuery.ToListAsync();

            // Prepare dictionary to group by sensor type
            var sensorTypeGroups = new Dictionary<string, List<int>>();

            foreach (var sensor in sensorInfos) {
                var typeName = sensor.SensorType?.TypeName ?? "Unknown";

                if (!sensorTypeGroups.ContainsKey(typeName)) {
                    sensorTypeGroups[typeName] = new List<int>();
                }

                sensorTypeGroups[typeName].Add(sensor.SensorInfoID);
            }

            // Get the latest reading for each monitored sensor type
            var sensorReadings = new List<SensorReadingData>();

            foreach (var typeGroup in sensorTypeGroups) {
                var typeName = typeGroup.Key;
                var sensorIds = typeGroup.Value;

                // Get all sensors of this type
                var sensorsOfType = sensorInfos
                    .Where(s => sensorIds.Contains(s.SensorInfoID))
                    .ToList();

                if (!sensorsOfType.Any())
                    continue;

                // Get the threshold info if available
                var thresholds = await _context.AlertThresholds
                    .Where(t => sensorIds.Contains(t.SensorInfoID))
                    .ToListAsync();

                // Calculate aggregate values for this sensor type
                var latestReadings = await _context.SensorReadings
                    .Where(r => sensorIds.Contains(r.SensorInfoID) &&
                                r.IsValid == true &&
                                r.ReadingTime > DateTime.Now.AddHours(-1))
                    .OrderByDescending(r => r.ReadingTime)
                    .GroupBy(r => r.SensorInfoID)
                    .Select(g => g.FirstOrDefault()) // Get latest reading for each sensor
                    .ToListAsync();

                // Get historical data for mini-charts (last 2 hours)
                var historicalData = await _context.SensorReadings
                    .Where(r => sensorIds.Contains(r.SensorInfoID) &&
                                r.IsValid == true &&
                                r.ReadingTime > DateTime.Now.AddHours(-2))
                    .OrderBy(r => r.ReadingTime)
                    .Select(r => new { r.ReadingTime, r.ReadingValue })
                    .ToListAsync();

                // Process historical data for chart
                var chartData = historicalData
                    .GroupBy(r => r.ReadingTime.Value.ToString("HH:mm"))
                    .Select(g => new ChartDataPoint2 {
                        Label = g.Key,
                        Value = (decimal)g.Average(r => r.ReadingValue ?? 0)
                    })
                    .OrderBy(p => p.Label)
                    .ToList();

                // Find min, max, and average thresholds across all sensors of this type
                decimal? minThreshold = null;
                decimal? maxThreshold = null;
                decimal? warningMinThreshold = null;
                decimal? warningMaxThreshold = null;

                if (thresholds.Any()) {
                    minThreshold = thresholds.Min(t => t.MinValue);
                    maxThreshold = thresholds.Max(t => t.MaxValue);
                    warningMinThreshold = thresholds.Min(t => t.WarningMinValue);
                    warningMaxThreshold = thresholds.Max(t => t.WarningMaxValue);
                }

                // Calculate current value (average of all latest readings)
                decimal currentValue = 0;
                string statusClass = "success";

                if (latestReadings.Any()) {
                    currentValue = (decimal)latestReadings.Average(r => r.ReadingValue ?? 0);

                    // Determine status based on thresholds
                    if (minThreshold.HasValue && currentValue < minThreshold.Value ||
                        maxThreshold.HasValue && currentValue > maxThreshold.Value) {
                        statusClass = "danger";
                    }
                    else if (warningMinThreshold.HasValue && currentValue < warningMinThreshold.Value ||
                             warningMaxThreshold.HasValue && currentValue > warningMaxThreshold.Value) {
                        statusClass = "warning";
                    }
                }

                // Get unit from sensor type
                var unit = sensorsOfType.FirstOrDefault()?.SensorType?.Unit ?? "";

                // Calculate gauge percentage
                int gaugePercentage = 50;
                if (minThreshold.HasValue && maxThreshold.HasValue) {
                    var range = maxThreshold.Value - minThreshold.Value;
                    if (range > 0) {
                        var position = currentValue - minThreshold.Value;
                        var percentage = (position / range) * 100;
                        gaugePercentage = Math.Min(100, Math.Max(0, (int)percentage));
                    }
                }
                else {
                    gaugePercentage = typeName.ToLower() switch {
                        "temperature" => (int)Math.Min(100, Math.Max(0, (currentValue / 50) * 100)),
                        "humidity" => (int)Math.Min(100, Math.Max(0, currentValue)),
                        "pressure" => (int)Math.Min(100, Math.Max(0, (currentValue / 1100) * 100)),
                        _ => 50
                    };
                }

                // Create data object for this sensor type
                var sensorReadingData = new SensorReadingData {
                    SensorType = typeName,
                    CurrentValue = currentValue,
                    Unit = unit,
                    StatusClass = statusClass,
                    MinThreshold = minThreshold,
                    MaxThreshold = maxThreshold,
                    WarningMinThreshold = warningMinThreshold,
                    WarningMaxThreshold = warningMaxThreshold,
                    ChartData = chartData,
                    SensorCount = sensorIds.Count,
                    ActiveSensorCount = latestReadings.Count,
                    LastUpdated = latestReadings.Any() ? latestReadings.Max(r => r.ReadingTime) : null,
                    Icon = typeName.ToLower() switch {
                        "temperature" => "bi-thermometer-half",
                        "humidity" => "bi-droplet",
                        "pressure" => "bi-speedometer",
                        _ => "bi-graph-up"
                    },
                    GaugePercentage = gaugePercentage
                };

                sensorReadings.Add(sensorReadingData);
            }

            // Create response object with room information
            var realTimeData = new RealTimeData {
                RoomId = roomId,
                RoomName = roomId > 0 ?
                    (await _context.CleanRooms.FindAsync(roomId))?.RoomName ?? "Unknown Room" :
                    "All Rooms",
                SensorReadings = sensorReadings,
                LastUpdated = DateTime.Now
            };

            // Sử dụng cấu hình serialization tùy chỉnh để tránh các đối tượng tham chiếu vòng
            var jsonResult = new JsonResult(realTimeData, _jsonOptions);
            return jsonResult;
        }
    }

    // Model classes for API response
    public class RealTimeData
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public List<SensorReadingData> SensorReadings { get; set; } = new List<SensorReadingData>();
        public DateTime LastUpdated { get; set; }
    }

    public class SensorReadingData
    {
        public string SensorType { get; set; }
        public decimal CurrentValue { get; set; }
        public string Unit { get; set; }
        public string StatusClass { get; set; }
        public decimal? MinThreshold { get; set; }
        public decimal? MaxThreshold { get; set; }
        public decimal? WarningMinThreshold { get; set; }
        public decimal? WarningMaxThreshold { get; set; }
        public List<ChartDataPoint2> ChartData { get; set; } = new List<ChartDataPoint2>();
        public int SensorCount { get; set; }
        public int ActiveSensorCount { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Icon { get; set; }
        public int GaugePercentage { get; set; }
    }

    public class ChartDataPoint2
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }
}