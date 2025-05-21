using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using CleanroomMonitoring.Web.ViewModels;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SystemHealthViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SystemHealthViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
          
            var tenMinutesAgo = DateTime.Now.AddMinutes(-10);

            // Calculate what percentage of sensors have reported in the last 10 minutes
            int totalActiveSensors = _context.SensorInfos 
                .Count(s => s.IsActive == true);

            //int reportingSensors = _context.SensorInfos
            //    .Count(s => s.IsActive == true && s.SensorReadings
            //        .Any(r => r.ReadingTime >= tenMinutesAgo));            

            int reportingSensors = _context.SensorConnectionStatuss
                     .Include(p => p.SensorInfo)
                .Count(s => s.IsConnected == true);

            double sensorReportingPercentage = totalActiveSensors > 0
                ? (double)reportingSensors / totalActiveSensors * 100
                : 0;

            var model = new SystemHealthViewModel {
                SensorUptime = sensorReportingPercentage,
                LastDataCollectionTime = _context.SensorReadings
                    .OrderByDescending(r => r.ReadingTime)
                    .FirstOrDefault()?.ReadingTime,
                OfflineSensors = totalActiveSensors - reportingSensors,
                DataPointsLast24Hours = _context.SensorReadings
                    .Count(r => r.ReadingTime >= DateTime.Now.AddHours(-24))
            };

            return View(model);
        }
    }
    public class SystemHealthViewModel
    {
        public double SensorUptime { get; set; }
        public DateTime? LastDataCollectionTime { get; set; }
        public int OfflineSensors { get; set; }
        public int DataPointsLast24Hours { get; set; }

        public string StatusClass
        {
            get
            {
                if (SensorUptime >= 95) return "success";
                if (SensorUptime >= 80) return "warning";
                return "danger";
            }
        }
    }

}