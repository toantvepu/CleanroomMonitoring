// 7. SensorTrendChartViewComponent.cs
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
    public class SensorTrendChartViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorTrendChartViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int sensorInfoId, int hoursBack = 24, string chartType = "line")
        {
            var sensor = await _context.SensorInfos
                .Include(s => s.SensorType)
                .FirstOrDefaultAsync(s => s.SensorInfoID == sensorInfoId);

            if (sensor == null) {
                return Content("Cảm biến không tồn tại");
            }

            // Lấy cấu hình cảm biến
            var sensorConfig = await _context.SensorConfigs
                .FirstOrDefaultAsync(c => c.SensorInfoID == sensorInfoId);

            // Lấy các đọc trong khoảng thời gian
            var fromDate = DateTime.Now.AddHours(-hoursBack);
            var readings = await _context.SensorReadings
                .Where(r => r.SensorInfoID == sensorInfoId && r.ReadingTime >= fromDate && r.IsValid == true)
                .OrderBy(r => r.ReadingTime)
                .ToListAsync();

            // Nhóm dữ liệu theo thời gian để giảm số điểm trên biểu đồ
            var groupedData = GroupReadingsByTimeInterval(readings, hoursBack);

            var viewModel = new SensorTrendChartViewModel {
                SensorInfo = sensor,
                SensorConfig = sensorConfig,
                Readings = groupedData,
                ChartType = chartType,
                HoursBack = hoursBack
            };

            return View(viewModel);
        }

        private List<SensorReading> GroupReadingsByTimeInterval(List<SensorReading> readings, int hoursBack)
        {
            if (readings.Count <= 100) return readings;

            // Tính toán khoảng thời gian phù hợp để có khoảng 100 điểm trên biểu đồ
            var totalMinutes = hoursBack * 60;
            var groupByMinutes = Math.Max(1, totalMinutes / 100);

            return readings
                .GroupBy(r => new {
                    Hour = r.ReadingTime?.Hour ?? 0,
                    Minute = (r.ReadingTime?.Minute ?? 0) / groupByMinutes * groupByMinutes
                })
                .Select(g => new SensorReading {
                    ReadingID = g.First().ReadingID,
                    SensorInfoID = g.First().SensorInfoID,
                    ReadingValue = g.Average(r => r.ReadingValue),
                    ReadingTime = g.Select(r => r.ReadingTime).OrderBy(t => t).First(),
                    IsValid = true
                })
                .ToList();
        }
    }

    public class SensorTrendChartViewModel
    {
        public SensorInfo SensorInfo { get; set; }
        public SensorConfig SensorConfig { get; set; }
        public List<SensorReading> Readings { get; set; }
        public string ChartType { get; set; }
        public int HoursBack { get; set; }
    }
}