using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class AlertsSummaryViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public AlertsSummaryViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var today = DateTime.Today;
            var model = new AlertsSummaryViewModel {
                TotalAlerts = await _context.AlertHistorys.CountAsync(a => a.AlertTime >= today.AddMonths(-1)),
                ActiveAlerts = await _context.AlertHistorys.CountAsync(a => a.AlertTime == null),
                AlertsToday = await _context.AlertHistorys.CountAsync(a => a.AlertTime >= today),
                CriticalAlerts = await _context.AlertHistorys.CountAsync(a => a.AlertType == "Critical" && a.AlertTime == null)
            };

            return View(model);
          
        }
    }

    public class AlertsSummaryViewModel
    {
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int AlertsToday { get; set; }
        public int CriticalAlerts { get; set; }
    }
}