using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;
using CleanroomMonitoring.Web.ViewModels;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorReadingsViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorReadingsViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? sensorInfoId = null, string readingType = null, int hoursBack = 24)
        {
            var query = _context.SensorInfos
                .Include(s => s.SensorType)
                .AsQueryable();

            if (sensorInfoId.HasValue) {
                query = query.Where(s => s.SensorInfoID == sensorInfoId.Value);
            }
            else if (!string.IsNullOrEmpty(readingType)) {
                // Lọc theo loại cảm biến (nhiệt độ, độ ẩm, áp suất)
                query = query.Where(s => s.SensorType.TypeName.Contains(readingType));
            }

            var sensorInfos = await query.ToListAsync();
            var fromDate = DateTime.Now.AddHours(-hoursBack);

            var viewModel = new SensorReadingsViewModel {
                SensorReadings = new List<SensorReadingData>()
            };

            foreach (var sensor in sensorInfos) {
                // Lấy cấu hình cảm biến
                var sensorConfig = await _context.SensorConfigs
                    .FirstOrDefaultAsync(c => c.SensorInfoID == sensor.SensorInfoID);

                // Lấy các đọc gần nhất
                var latestReading = await _context.SensorReadings
                    .Where(r => r.SensorInfoID == sensor.SensorInfoID)
                    .OrderByDescending(r => r.ReadingTime)
                    .FirstOrDefaultAsync();

                // Lấy tất cả các đọc trong khoảng thời gian
                var readings = await _context.SensorReadings
                    .Where(r => r.SensorInfoID == sensor.SensorInfoID && r.ReadingTime >= fromDate)
                    .OrderBy(r => r.ReadingTime)
                    .ToListAsync();

                var readingData = new SensorReadingData {
                    SensorInfo = sensor,
                    LatestReading = latestReading,
                    Readings = readings,
                    SensorConfig = sensorConfig,
                    MinValue = readings.Any() ? readings.Min(r => r.ReadingValue) ?? 0 : 0,
                    MaxValue = readings.Any() ? readings.Max(r => r.ReadingValue) ?? 0 : 0,
                    AvgValue = readings.Any() ? readings.Average(r => r.ReadingValue ?? 0) : 0
                };

                viewModel.SensorReadings.Add(readingData);
            }

            return View(viewModel);
        }
    }
}