using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.ViewModels;
using Newtonsoft.Json;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorReadingsChartViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorReadingsChartViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string range = "24h", int interval = 15, string area = "ALL")
        {
            Console.WriteLine($"[DEBUG] ViewComponent loaded with range={range}, interval={interval}, area={area}");

            DateTime fromTime = range switch {
                "7d" => DateTime.Now.AddDays(-7),
                "30d" => DateTime.Now.AddDays(-30),
                "24h" => DateTime.Now.AddHours(-24),
                "8h" => DateTime.Now.AddHours(-8),
                "2h" => DateTime.Now.AddHours(-2),
                _ => DateTime.Now.AddHours(-24),
            };
            
            //Truy vấn và nhóm dữ liệu theo interval
            var temperatureReadings = await _context.SensorReadings
                  .Include(r => r.SensorInfo) 
                .ThenInclude(s => s.SensorType)
                .Where(r => r.ReadingTime >= fromTime 
                        && (area=="ALL" || r.SensorInfo.CleanRoom.Area== area) 
                        && r.SensorInfo.SensorType.TypeName.Contains("Temperature")
                       )
                .GroupBy(r => new {
                    TimeGroup = EF.Functions.DateDiffMinute(fromTime, r.ReadingTime) / interval
                })
                .Select(g => new {
                    Time = g.Min(r => r.ReadingTime),
                    Value = g.Average(r => r.ReadingValue)
                })
                .OrderBy(g => g.Time)

                .ToListAsync();


            var humidityReadings = await _context.SensorReadings
                  .Include(r => r.SensorInfo)
                .ThenInclude(s => s.SensorType)
                .Where(r => r.ReadingTime >= fromTime
                && (area == "ALL" || r.SensorInfo.CleanRoom.Area == area)
                && r.SensorInfo.SensorType.TypeName.Contains("Humidity"))
                .GroupBy(r => new {
                    TimeGroup = EF.Functions.DateDiffMinute(fromTime, r.ReadingTime) / interval
                })
                .Select(g => new {
                    Time = g.Min(r => r.ReadingTime),
                    Value = g.Average(r => r.ReadingValue)
                })
                .OrderBy(g => g.Time)
                .ToListAsync();


            var pressureReadings = await _context.SensorReadings
                 .Include(r => r.SensorInfo)
               .ThenInclude(s => s.SensorType)
               .Where(r => r.ReadingTime >= fromTime
               && (area == "ALL" || r.SensorInfo.CleanRoom.Area == area)
               && r.SensorInfo.SensorType.TypeName.Contains("Pressure"))
               .GroupBy(r => new {
                   TimeGroup = EF.Functions.DateDiffMinute(fromTime, r.ReadingTime) / interval
               })
               .Select(g => new {
                   Time = g.Min(r => r.ReadingTime),
                   Value = g.Average(r => r.ReadingValue)
               })
               .OrderBy(g => g.Time)
               .ToListAsync();
 

            var model = new SensorReadingsChartViewModel {

                TemperatureReadings = temperatureReadings
                    .Select(r => new ReadingData {
                        Time = r.Time.HasValue ? r.Time.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                        Value = Math.Round(r.Value??0)
                    }).ToList(),
                HumidityReadings = humidityReadings
                     .Select(r => new ReadingData {
                         Time = r.Time.HasValue ? r.Time.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                         Value = Math.Round(r.Value ?? 0)
                     }).ToList(),

                PressureReadings = pressureReadings
                     .Select(r => new ReadingData {
                         Time = r.Time.HasValue ? r.Time.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty,
                         Value = Math.Round(r.Value ?? 0)
                     }).ToList()
            };
            // Log data for debugging
            Console.WriteLine("Temperature Readings: " + JsonConvert.SerializeObject(model.TemperatureReadings));
            Console.WriteLine("Humidity Readings: " + JsonConvert.SerializeObject(model.HumidityReadings));
            Console.WriteLine("Pressure Readings: " + JsonConvert.SerializeObject(model.PressureReadings));

            ViewData["RecordCount"] = model.TemperatureReadings.Count; // gửi số lượng bản ghi
            ViewData["fromtime"] = model.TemperatureReadings[0].Time;  
            ViewData["totime"] = model.TemperatureReadings[model.TemperatureReadings.Count-1].Time;


            //Tính toán các giá trị trung bình, lớn nhất, nhỏ nhất
            ViewData["maxTemperature"] = Math.Round(model.TemperatureReadings.Max(p => p.Value) ?? 0, 1);
            ViewData["minTemperature"] = Math.Round(model.TemperatureReadings.Min(p => p.Value) ?? 0, 1);
            ViewData["avgTemperature"] = Math.Round(model.TemperatureReadings.Average(p => p.Value) ?? 0, 1);

            ViewData["maxHumidity"] = Math.Round(model.HumidityReadings.Max(p => p.Value) ?? 0, 1);
            ViewData["minHumidity"] = Math.Round(model.HumidityReadings.Min(p => p.Value) ?? 0, 1);
            ViewData["avgHumidity"] = Math.Round(model.HumidityReadings.Average(p => p.Value) ?? 0, 1);

            ViewData["maxPressure"] = Math.Round(model.PressureReadings.Max(p => p.Value) ?? 0, 1);
            ViewData["minPressure"] = Math.Round(model.PressureReadings.Min(p => p.Value) ?? 0, 1);
            ViewData["avgPressure"] = Math.Round(model.PressureReadings.Average(p => p.Value) ?? 0, 1);

            //đếm lỗi theo từng loại (Temperature, Humidity, Pressure)
            var errorTemperature = await _context.SensorReadings
            .Include(r => r.SensorInfo)
            .ThenInclude(s => s.SensorType)
            .Where(r => r.ReadingTime >= fromTime
                && (area == "ALL" || r.SensorInfo.CleanRoom.Area == area)
                && r.SensorInfo.SensorType.TypeName.Contains("Temperature")
                && r.IsValid == false)
            .CountAsync();

            var errorHumidity = await _context.SensorReadings
                .Include(r => r.SensorInfo)
                .ThenInclude(s => s.SensorType)
                .Where(r => r.ReadingTime >= fromTime
                    && (area == "ALL" || r.SensorInfo.CleanRoom.Area == area)
                    && r.SensorInfo.SensorType.TypeName.Contains("Humidity")
                    && r.IsValid == false)
                .CountAsync();

            var errorPressure = await _context.SensorReadings
                .Include(r => r.SensorInfo)
                .ThenInclude(s => s.SensorType)
                .Where(r => r.ReadingTime >= fromTime
                    && (area == "ALL" || r.SensorInfo.CleanRoom.Area == area)
                    && r.SensorInfo.SensorType.TypeName.Contains("Pressure")
                    && r.IsValid == false)
                .CountAsync();

            ViewData["ErrorTemperature"] = errorTemperature;
            ViewData["ErrorHumidity"] = errorHumidity;
            ViewData["ErrorPressure"] = errorPressure;

            return View(model);
        }
    }
    public class SensorReadingsChartViewModel
    {
        public List<ReadingData> TemperatureReadings { get; set; } = new List<ReadingData>();
        public List<ReadingData> HumidityReadings { get; set; } = new List<ReadingData>();
        public List<ReadingData> PressureReadings { get; set; } = new List<ReadingData>();
    }
    public class ReadingData
    {
        public string Time { get; set; } = string.Empty; // Initialize with a default value to avoid CS8618
        public decimal? Value { get; set; }
    }
}