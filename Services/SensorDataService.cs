using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.Services
{
    /// <summary>
    /// Triển khai các chức năng xử lý dữ liệu cảm biến trong hệ thống giám sát phòng sạch
    /// </summary>
    public class SensorDataService : ISensorDataService
    {
        private readonly dbDataContext _context;

        /// <summary>
        /// Khởi tạo service với context cơ sở dữ liệu
        /// </summary>
        /// <param name="context">DbContext để truy cập cơ sở dữ liệu</param>
        public SensorDataService(dbDataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả các dữ liệu đọc từ một cảm biến trong khoảng thời gian
        /// </summary>
        public async Task<IEnumerable<SensorReading>> GetSensorReadingsAsync(int sensorId, DateTime startTime, DateTime endTime)
        {
            return await _context.SensorReadings
                .Where(sr => sr.SensorInfoID == sensorId &&
                       sr.ReadingTime >= startTime &&
                       sr.ReadingTime <= endTime)
                .OrderBy(sr => sr.ReadingTime)
                .ToListAsync();
        }

        /// <summary>
        /// Tính toán các thống kê cơ bản cho dữ liệu từ một cảm biến
        /// </summary>
        public async Task<Dictionary<string, object>> GetSensorStatisticsDictionaryAsync(int sensorId, DateTime startTime, DateTime endTime)
        {
            var readings = await _context.SensorReadings
                .Where(sr => sr.SensorInfoID == sensorId &&
                       sr.ReadingTime >= startTime &&
                       sr.ReadingTime <= endTime &&
                       sr.ReadingValue.HasValue)
                .Select(sr => sr.ReadingValue.Value)
                .ToListAsync();

            if (!readings.Any()) {
                return new Dictionary<string, object>
                {
                    { "min", null },
                    { "max", null },
                    { "avg", null },
                    { "count", 0 }
                };
            }

            return new Dictionary<string, object>
            {
                { "min", readings.Min() },
                { "max", readings.Max() },
                { "avg", readings.Average() },
                { "count", readings.Count }
            };
        }

        /// <summary>
        /// Kiểm tra xem cảm biến có đang trong trạng thái cảnh báo không
        /// dựa trên chỉ số IsValid của bản ghi gần nhất
        /// </summary>
        public async Task<bool> IsSensorInAlertStateAsync(int sensorId)
        {
            var latestReading = await _context.SensorReadings
                .Where(sr => sr.SensorInfoID == sensorId)
                .OrderByDescending(sr => sr.ReadingTime)
                .FirstOrDefaultAsync();

            return latestReading != null && latestReading.IsValid == false;
        }

        /// <summary>
        /// Cung cấp các thống kê chi tiết của một cảm biến
        /// </summary>
        public async Task<SensorStatistics> GetSensorStatisticsAsync(int sensorId, DateTime startDate, DateTime endDate)
        {
            var readings = await _context.SensorReadings
                .Where(sr => sr.SensorInfoID == sensorId &&
                       sr.ReadingTime >= startDate &&
                       sr.ReadingTime <= endDate)
                .ToListAsync();

            if (!readings.Any()) {
                return new SensorStatistics();
            }

            var validReadings = readings.Where(r => r.IsValid ?? true).ToList();
            var invalidReadings = readings.Where(r => !(r.IsValid ?? true)).ToList();
            var validValues = validReadings.Where(r => r.ReadingValue.HasValue).Select(r => r.ReadingValue.Value).ToList();

            // Tính toán thống kê
            var stats = new SensorStatistics {
                TotalReadings = readings.Count,
                ValidReadings = validReadings.Count,
                InvalidReadings = invalidReadings.Count
            };

            if (validValues.Any()) {
                stats.MinValue = validValues.Min();
                stats.MaxValue = validValues.Max();
                stats.AvgValue = validValues.Average();

                // Tính độ lệch chuẩn
                double mean = (double)stats.AvgValue;
                double sumOfSquaresOfDifferences = validValues.Sum(val => Math.Pow((double)val - mean, 2));
                double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / validValues.Count);
                stats.StdDeviation = (decimal)standardDeviation;

                // Tính số lượng cảnh báo (giá trị vượt quá 3 độ lệch chuẩn so với giá trị trung bình)
                double threshold = 3 * standardDeviation;
                stats.AlertCount = validValues.Count(val => Math.Abs((double)val - mean) > threshold);
            }

            return stats;
        }

        /// <summary>
        /// Lấy tất cả các sự kiện cảnh báo từ các cảm biến trong một phòng
        /// dựa trên ngưỡng giới hạn cho từng loại cảm biến
        /// </summary>
        public async Task<Dictionary<int, List<AlertEvent>>> GetRoomAlertsAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            // Lấy tất cả các cảm biến trong phòng
            var sensors = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Where(s => s.RoomID == roomId)
                .ToListAsync();

            var result = new Dictionary<int, List<AlertEvent>>();

            foreach (var sensor in sensors) {
                // Lấy tất cả các bản ghi của cảm biến trong khoảng thời gian
                var readings = await _context.SensorReadings
                    .Where(sr => sr.SensorInfoID == sensor.SensorInfoID &&
                           sr.ReadingTime >= startDate &&
                           sr.ReadingTime <= endDate)
                    .OrderBy(sr => sr.ReadingTime)
                    .ToListAsync();

                // Thiết lập ngưỡng giới hạn cho từng loại cảm biến
                decimal lowThreshold = 0;
                decimal highThreshold = 100;

                // Xác định ngưỡng phù hợp cho từng loại cảm biến
                if (sensor.SensorType.TypeName.Contains("Temperature", StringComparison.OrdinalIgnoreCase)) {
                    lowThreshold = 10; // 18°C
                    highThreshold = 50; // 25°C
                }
                else if (sensor.SensorType.TypeName.Contains("Humidity", StringComparison.OrdinalIgnoreCase)) {
                    lowThreshold = 10; // 30%
                    highThreshold = 80; // 60%
                }
                else if (sensor.SensorType.TypeName.Contains("Pressure", StringComparison.OrdinalIgnoreCase)) {
                    lowThreshold = 990; // 990 hPa
                    highThreshold = 1030; // 1030 hPa
                }
                else if (sensor.SensorType.TypeName.Contains("Particle", StringComparison.OrdinalIgnoreCase)) {
                    lowThreshold = 0;
                    highThreshold = 10000; // particles/m³
                }

                var alertEvents = new List<AlertEvent>();
                AlertEvent currentAlert = null;

                foreach (var reading in readings) {
                    if (reading.ReadingValue.HasValue && reading.ReadingTime.HasValue) {
                        bool inAlertState = false;
                        string alertType = "";

                        // Kiểm tra nếu giá trị nằm ngoài ngưỡng
                        if (reading.ReadingValue.Value > highThreshold) {
                            inAlertState = true;
                            alertType = "High";
                        }
                        else if (reading.ReadingValue.Value < lowThreshold) {
                            inAlertState = true;
                            alertType = "Low";
                        }
                        // Kiểm tra nếu bản ghi được đánh dấu là không hợp lệ
                        else if (!(reading.IsValid ?? true)) {
                            inAlertState = true;
                            alertType = "Invalid";
                        }

                        // Bắt đầu một cảnh báo mới nếu cần
                        if (inAlertState && currentAlert == null) {
                            currentAlert = new AlertEvent {
                                SensorInfoID = sensor.SensorInfoID,
                                SensorName = sensor.SensorName,
                                StartTime = reading.ReadingTime.Value,
                                AlertType = alertType,
                                ThresholdValue = alertType == "High" ? highThreshold : lowThreshold,
                                ActualValue = reading.ReadingValue.Value
                            };
                        }
                        // Kết thúc cảnh báo hiện tại nếu không còn trong trạng thái cảnh báo
                        else if (!inAlertState && currentAlert != null) {
                            currentAlert.EndTime = reading.ReadingTime.Value;
                            currentAlert.Duration = currentAlert.EndTime.Value - currentAlert.StartTime;
                            alertEvents.Add(currentAlert);
                            currentAlert = null;
                        }
                    }
                }

                // Xử lý trường hợp bản ghi cuối cùng vẫn còn trong trạng thái cảnh báo
                if (currentAlert != null) {
                    currentAlert.EndTime = endDate;
                    currentAlert.Duration = currentAlert.EndTime.Value - currentAlert.StartTime;
                    alertEvents.Add(currentAlert);
                }

                if (alertEvents.Any()) {
                    result.Add(sensor.SensorInfoID, alertEvents);
                }
            }

            return result;
        }

        /// <summary>
        /// Lấy dữ liệu đọc từ cảm biến dưới dạng các điểm dữ liệu biểu đồ
        /// </summary>
        public async Task<List<ChartDataPoint>> GetSensorReadingsDataPointAsync(int sensorId, DateTime startDate, DateTime endDate)
        {
            var readings = await _context.SensorReadings
                .Where(sr => sr.SensorInfoID == sensorId &&
                       sr.ReadingTime >= startDate &&
                       sr.ReadingTime <= endDate &&
                       sr.ReadingValue.HasValue)
                .OrderBy(sr => sr.ReadingTime)
                .Select(sr => new ChartDataPoint {
                    Timestamp = sr.ReadingTime ?? DateTime.MinValue,
                    Value = sr.ReadingValue ?? 0
                })
                .ToListAsync();

            return readings;
        }

        /// <summary>
        /// Tính giá trị trung bình theo từng giờ trong ngày cho dữ liệu cảm biến
        /// </summary>
        public async Task<Dictionary<string, List<ChartDataPoint>>> GetHourlyAveragesAsync(int sensorId, DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<string, List<ChartDataPoint>>();

            // Lấy tất cả các bản ghi của cảm biến trong khoảng thời gian
            var readings = await _context.SensorReadings
                .Where(sr => sr.SensorInfoID == sensorId &&
                       sr.ReadingTime >= startDate &&
                       sr.ReadingTime <= endDate &&
                       sr.ReadingValue.HasValue)
                .OrderBy(sr => sr.ReadingTime)
                .ToListAsync();

            if (!readings.Any()) {
                return result;
            }

            // Nhóm các bản ghi theo giờ trong ngày
            var hourlyAverages = new List<ChartDataPoint>();
            for (int hour = 0; hour < 24; hour++) {
                var hourReadings = readings
                    .Where(r => r.ReadingTime.HasValue && r.ReadingTime.Value.Hour == hour)
                    .Select(r => r.ReadingValue.Value);

                if (hourReadings.Any()) {
                    hourlyAverages.Add(new ChartDataPoint {
                        Timestamp = new DateTime(2000, 1, 1, hour, 0, 0), // Chỉ quan tâm đến giờ
                        Value = hourReadings.Average()
                    });
                }
            }

            result.Add("hourly", hourlyAverages);
            return result;
        }

        /// <summary>
        /// Tính giá trị trung bình theo ngày cho dữ liệu cảm biến
        /// </summary>
        public async Task<List<ChartDataPoint>> GetDailyAveragesAsync(int sensorId, DateTime startDate, DateTime endDate)
        {
            var dailyAverages = new List<ChartDataPoint>();
            var dayRange = (endDate - startDate).Days + 1;

            for (int i = 0; i < dayRange; i++) {
                var day = startDate.AddDays(i);
                var nextDay = day.AddDays(1);

                var dayAverage = await _context.SensorReadings
                    .Where(sr => sr.SensorInfoID == sensorId &&
                           sr.ReadingTime >= day &&
                           sr.ReadingTime < nextDay &&
                           sr.ReadingValue.HasValue)
                    .Select(sr => sr.ReadingValue.Value)
                    .DefaultIfEmpty()
                    .AverageAsync();

                dailyAverages.Add(new ChartDataPoint {
                    Timestamp = day,
                    Value = dayAverage
                });
            }

            return dailyAverages;
        }
    }
}