

using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class RoomStatusViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public RoomStatusViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //var rooms = await _context.CleanRooms
            //    .Include(r => r.SensorInfos)
            //    .ThenInclude(s => s.SensorReadings.OrderByDescending(sr => sr.ReadingTime).Take(1))
            //    .OrderBy(r => r.RoomName)
            //    .Take(10)
            //    .Select(r => new RoomStatusViewModel {
            //        RoomID = r.RoomID,
            //        RoomName = r.RoomName,
            //        CleanRoomClass = r.CleanRoomClass,
            //        Area = r.Area,
            //        HasActiveSensors = r.SensorInfos.Any(s => s.IsActive == true),
            //        SensorCount = r.SensorInfos.Count,
            //        HasActiveAlerts = _context.AlertHistorys
            //            .Any(a => a.SensorInfo.RoomID == r.RoomID && a.ResolvedTime == null),
            //        TemperatureReading = r.SensorInfos
            //            .Where(s => s.SensorType.TypeName.Contains("Temperature"))
            //            .SelectMany(s => s.SensorReadings)
            //            .OrderByDescending(sr => sr.ReadingTime)
            //            .FirstOrDefault() != null ?
            //                r.SensorInfos
            //                    .Where(s => s.SensorType.TypeName.Contains("Temperature"))
            //                    .SelectMany(s => s.SensorReadings)
            //                    .OrderByDescending(sr => sr.ReadingTime)
            //                    .FirstOrDefault().ReadingValue : (decimal?)null,
            //        HumidityReading = r.SensorInfos
            //            .Where(s => s.SensorType.TypeName.Contains("Humidity"))
            //            .SelectMany(s => s.SensorReadings)
            //            .OrderByDescending(sr => sr.ReadingTime)
            //            .FirstOrDefault() != null ?
            //                r.SensorInfos
            //                    .Where(s => s.SensorType.TypeName.Contains("Humidity"))
            //                    .SelectMany(s => s.SensorReadings)
            //                    .OrderByDescending(sr => sr.ReadingTime)
            //                    .FirstOrDefault().ReadingValue : (decimal?)null,
            //        PressureReading = r.SensorInfos
            //            .Where(s => s.SensorType.TypeName.Contains("Pressure"))
            //            .SelectMany(s => s.SensorReadings)
            //            .OrderByDescending(sr => sr.ReadingTime)
            //            .FirstOrDefault() != null ?
            //                r.SensorInfos
            //                    .Where(s => s.SensorType.TypeName.Contains("Pressure"))
            //                    .SelectMany(s => s.SensorReadings)
            //                    .OrderByDescending(sr => sr.ReadingTime)
            //                    .FirstOrDefault().ReadingValue : (decimal?)null
            //    })
            //    .ToListAsync();

            //return View(rooms);
            return View();
        }

 
    }
    public class RoomStatusViewModel
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; }
        public string CleanRoomClass { get; set; }
        public string Area { get; set; }
        public bool HasActiveSensors { get; set; }
        public int SensorCount { get; set; }
        public bool HasActiveAlerts { get; set; }
        public decimal? TemperatureReading { get; set; }
        public decimal? HumidityReading { get; set; }
        public decimal? PressureReading { get; set; }

        public string StatusClass => HasActiveAlerts ? "danger" : (HasActiveSensors ? "success" : "warning");
    }

}