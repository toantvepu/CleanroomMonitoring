using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CleanroomMonitoring.Web.Data;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorPerformanceViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorPerformanceViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 8)
        {
            IQueryable<SensorInfo> query = _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SensorReadings.Count(r => r.IsValid == false));
            if (count > 0)
                query = query.Take(count);
             
            var sensors = await query
                .Select(s => new SensorPerformanceViewModel {
                    SensorID = s.SensorInfoID,
                    SensorName = s.SensorName,
                    SensorType = s.SensorType.TypeName,
                    RoomName = s.CleanRoom.RoomName,
                    TotalReadings = s.SensorReadings.Count(),
                    InvalidReadings = s.SensorReadings.Count(r => r.IsValid == false),
                    LastReading = s.SensorReadings
                        .OrderByDescending(r => r.ReadingTime)
                        .FirstOrDefault() != null ?
                            s.SensorReadings
                                .OrderByDescending(r => r.ReadingTime)
                                .FirstOrDefault().ReadingValue : (decimal?)null,
                    LastReadingTime = s.SensorReadings
                        .OrderByDescending(r => r.ReadingTime)
                        .FirstOrDefault() != null ?
                            s.SensorReadings
                                .OrderByDescending(r => r.ReadingTime)
                                .FirstOrDefault().ReadingTime : (DateTime?)null
                })
                .ToListAsync();

            foreach (var sensor in sensors) {
                sensor.CalculatePerformance();
            }

            return View(sensors);
        }
    }

    public class SensorPerformanceViewModel
    {
        public int SensorID { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string RoomName { get; set; }
        public int TotalReadings { get; set; }
        public int InvalidReadings { get; set; }
        public decimal? LastReading { get; set; }
        public DateTime? LastReadingTime { get; set; }
        public double Performance { get; set; }

        public void CalculatePerformance()
        {
            if (TotalReadings > 0) {
                Performance = 100 - ((double)InvalidReadings / TotalReadings * 100);
            }
        }

        public string GetPerformanceClass()
        {
            if (Performance >= 95) return "success";
            if (Performance >= 80) return "warning";
            return "danger";
        }
    }
}