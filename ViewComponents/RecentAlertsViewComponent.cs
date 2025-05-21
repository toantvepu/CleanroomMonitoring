using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class RecentAlertsViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;
        public RecentAlertsViewComponent(dbDataContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recentAlerts = await _context.AlertHistorys
         .Include(a => a.SensorInfo)
         .ThenInclude(s => s.CleanRoom) 
         .OrderByDescending(a => a.AlertTime)
         .Take(10)
         .Select(a => new AlertViewModel {
             AlertID = a.AlertID,
             AlertType = a.AlertType,
             AlertMessage = a.AlertMessage,
             AlertTime = a.AlertTime,
             RoomName = a.SensorInfo.CleanRoom.RoomName,
             SensorName = a.SensorInfo.SensorName,
             ReadingValue = a.AlertValue,
             IsResolved = a.IsHandled  
         })
         .ToListAsync();

            return View(recentAlerts);
        }
    }

}
