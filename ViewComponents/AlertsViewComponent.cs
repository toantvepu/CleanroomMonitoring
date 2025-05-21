// 3. AlertsViewComponent.cs
using System;
using System.Linq;
using System.Threading.Tasks;

using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class AlertsViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public AlertsViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? roomId = null, int maxAlerts = 10)
        {
            var query = _context.AlertHistorys
                .Include(a => a.SensorInfo)
                .ThenInclude(s => s.SensorType)
                .Include(a => a.SensorInfo.CleanRoom)
                .OrderByDescending(a => a.AlertTime)
                .AsQueryable();

            if (roomId.HasValue) {
                query = query.Where(a => a.SensorInfo.RoomID == roomId.Value);
            }

            var alerts = await query.Take(maxAlerts).ToListAsync();

            // Thông tin về các cảnh báo chưa xử lý
            var unacknowledgedAlerts = await query
                .Where(a => !a.IsHandled)
                .CountAsync();

            var viewModel = new AlertsViewModel {
                Alerts = alerts,
                UnacknowledgedAlerts = unacknowledgedAlerts
            };

            return View(viewModel);
        }
    }

    public class AlertsViewModel
    {
        public List<AlertHistory> Alerts { get; set; }
        public int UnacknowledgedAlerts { get; set; }
    }
}