using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorStatusViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorStatusViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? roomId = null)
        {
            var query = _context.SensorInfos
                .Include(s => s.SensorType)
                .AsQueryable();

            if (roomId.HasValue) {
                query = query.Where(s => s.RoomID == roomId.Value);
            }

            var sensorInfos = await query.ToListAsync();

            // Lấy trạng thái kết nối của các sensor
            var sensorConnectionStatuses = await _context.SensorConnectionStatuss
                .Where(s => roomId == null ||
                       _context.SensorInfos.Where(si => si.RoomID == roomId).Select(si => si.SensorInfoID).Contains(s.SensorInfoID))
                .ToListAsync();

            // Tính toán thống kê trạng thái
            var totalSensors = sensorInfos.Count;
            var connectedSensors = sensorConnectionStatuses.Count(s => s.IsConnected);
            var disconnectedSensors = totalSensors - connectedSensors;

            var viewModel = new SensorStatusViewModel2 {
                TotalSensors = totalSensors,
                ConnectedSensors = connectedSensors,
                DisconnectedSensors = disconnectedSensors,
                SensorStatuses = sensorConnectionStatuses
            };

            return View(viewModel);
        }
    }
 }
