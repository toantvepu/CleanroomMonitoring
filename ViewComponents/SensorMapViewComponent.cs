using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorMapViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorMapViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string area= "1F")
        {
            string imageFile = area + ".png"; // ví dụ: "1F.png", "2F.png", "3F.png"
            // Lấy tất cả phòng thuộc Area
            var rooms = await _context.CleanRooms
                .Where(r => r.Area.Contains(area))
                .ToListAsync();

            var roomIds = rooms.Select(r => r.RoomID).ToList();

            // Lấy tất cả sensor thuộc các phòng này
            var sensors = await _context.SensorInfos
                .Where(s => s.RoomID != null && roomIds.Contains(s.RoomID.Value))
                .Include(s => s.SensorType)
                .ToListAsync();

            var sensorIds = sensors.Select(s => s.SensorInfoID).ToList();

            // Lấy vị trí overlay (SensorLocation) cho từng sensor
            var locations = await _context.Set<SensorLocation>()
                .Where(l => l.SensorInfoID != null && sensorIds.Contains(l.SensorInfoID.Value))
                .ToListAsync();

            // Lấy reading mới nhất cho từng sensor
            var latestReadings = await _context.SensorReadings
                .Where(r => sensorIds.Contains(r.SensorInfoID))
                .GroupBy(r => r.SensorInfoID)
                .Select(g => g.OrderByDescending(r => r.ReadingTime).FirstOrDefault())
                .ToListAsync();

            // Ví dụ mapping thủ công. Chỉ hướng cho một số id gần nhau quá thôi. Các vị trí rộng rãi thì thôi. Làm lâu vãi L.
           

            var overlays = (from sensor in sensors
                            join room in rooms on sensor.RoomID equals room.RoomID
                            join loc in locations on sensor.SensorInfoID equals loc.SensorInfoID
                            join reading in latestReadings on sensor.SensorInfoID equals reading.SensorInfoID into readingGroup
                            from reading in readingGroup.DefaultIfEmpty()
                            where loc.IsActive==true
                            select new { 
                                room.RoomID,
                                room.RoomName,
                                X = loc.ToaDoX ?? 0,
                                Y = loc.ToaDoY ?? 0,
                                OverlayDirection=loc.OverlayDirection,
                                LocationName=loc.LocationName,
                                Comment=sensor.COMMENT, //lấy Point
                                SensorType = sensor.SensorType.TypeName,
                                Value = reading?.ReadingValue,
                                Unit = sensor.SensorType.Unit,
                                ReadingTime =reading.ReadingTime
                            })
                .GroupBy(x => new { x.RoomID, x.RoomName, x.X, x.Y,x.OverlayDirection,x.Comment })
                .Select(g => new RoomSensorOverlayGroupViewModel {
                    RoomID = g.Key.RoomID,
                    RoomName = g.Key.RoomName,
                    X = g.Key.X,
                    Y = g.Key.Y,
                    OverlayDirection = g.Key.OverlayDirection,
                    Comment = g.Key.Comment,
                    ImageFile = imageFile,
                    Sensors = g.Select(s => new RoomSensorOverlayGroupViewModel.SensorValue {
                        SensorType = s.SensorType,
                        Value = s.Value,
                        Unit = s.Unit,
                        ReadingTime =s.ReadingTime,
                        LocationName=s.LocationName,
                        
                    }).ToList()
                }).ToList();
            if (overlays.Count>0) {
                ViewData["ReadingTime"] = overlays[0].Sensors[0].ReadingTime;
            }
          

            return View(overlays);
        }
    }
    

    public class RoomSensorOverlayGroupViewModel
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string? OverlayDirection { get; set; } = "top"; // "left", "right", "top", "bottom"
        public string? Comment { get; set; } //Lấy ra Point
        public List<SensorValue> Sensors { get; set; }
        public string ImageFile { get; set; } // Thêm dòng này
        public class SensorValue
        {
            public string LocationName { get; set; }
           
            public string SensorType { get; set; }
            public decimal? Value { get; set; }
            public string Unit { get; set; }
            public bool? IsValid { get; set; }
            public DateTime? ReadingTime { get; set; }
        }
    }

}
