using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class CriticalAlertsViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public CriticalAlertsViewComponent (dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int maxAlerts = 5)
        {
            // Get critical alerts - focus on:
            // 1. Unhandled alerts (IsHandled = false or null)
            // 2. Recent alerts (within last 24 hours)
            // 3. Critical alert types (based on AlertType)
            var criticalAlerts = await _context.AlertHistorys
                .Include(a => a.SensorInfo)
                    .ThenInclude(s => s.SensorType)
                .Include(a => a.SensorInfo)
                    .ThenInclude(s => s.CleanRoom)
                .Where(a => (a.IsHandled == false || a.IsHandled == null) &&
                            a.AlertTime >= DateTime.Now.AddHours(-24))
                .OrderByDescending(a =>
                    // Prioritize critical alerts first
                    a.AlertType == "Critical" ? 0 :
                    a.AlertType == "Warning" ? 1 : 2)
                .ThenByDescending(a => a.AlertTime) // Then by most recent
                .Take(maxAlerts)
                .Select(a => new CriticalAlertViewModel {
                    AlertID = a.AlertID,
                    SensorName = a.SensorInfo.SensorName,
                    RoomName = a.SensorInfo.CleanRoom.RoomName,
                    RoomID = a.SensorInfo.RoomID ?? 0,
                    AlertTime = a.AlertTime,
                    AlertType = a.AlertType,
                    AlertMessage = a.AlertMessage,
                    AlertValue = a.AlertValue,
                    SensorType = a.SensorInfo.SensorType.TypeName,
                    Unit = a.SensorInfo.SensorType.Unit,
                    TimeSinceAlert = a.AlertTime.HasValue ?
                        GetTimeSpanDisplay(DateTime.Now - a.AlertTime.Value) :
                        "Unknown",
                    SensorInfoID = a.SensorInfoID
                })
                .ToListAsync();

            // Get alert count by type for the summary
            var alertSummary = new AlertSummaryViewModel {
                TotalUnhandledAlerts = await _context.AlertHistorys
                    .CountAsync(a => a.IsHandled == false || a.IsHandled == null),
                CriticalAlertCount = await _context.AlertHistorys
                    .CountAsync(a => (a.IsHandled == false || a.IsHandled == null) &&
                                a.AlertType == "Critical"),
                WarningAlertCount = await _context.AlertHistorys
                    .CountAsync(a => (a.IsHandled == false || a.IsHandled == null) &&
                                a.AlertType == "Warning"),
                InfoAlertCount = await _context.AlertHistorys
                    .CountAsync(a => (a.IsHandled == false || a.IsHandled == null) &&
                                a.AlertType == "Info")
            };

            var viewModel = new CriticalAlertsViewModel {
                Alerts = criticalAlerts,
                Summary = alertSummary
            };

            return View(viewModel);
        }

        // Made static to avoid the EF Core error
        private static string GetTimeSpanDisplay(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h ago";
            else if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m ago";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            else
                return "Just now";
        }
    }

    public class CriticalAlertViewModel
    {
        public long AlertID { get; set; }
        public string SensorName { get; set; }
        public string RoomName { get; set; }
        public int RoomID { get; set; }
        public DateTime? AlertTime { get; set; }
        public string AlertType { get; set; }
        public string AlertMessage { get; set; }
        public decimal? AlertValue { get; set; }
        public string SensorType { get; set; }
        public string Unit { get; set; }
        public string TimeSinceAlert { get; set; }
        public int SensorInfoID { get; set; }

        // Helper property to determine alert severity CSS class
        public string SeverityClass
        {
            get
            {
                return AlertType?.ToLower() switch {
                    "critical" => "danger",
                    "warning" => "warning",
                    "info" => "info",
                    _ => "secondary"
                };
            }
        }

        // Helper property to determine icon
        public string AlertIcon
        {
            get
            {
                return SensorType?.ToLower() switch {
                    "temperature" => "bi-thermometer-high",
                    "humidity" => "bi-droplet",
                    "pressure" => "bi-speedometer",
                    _ => "bi-exclamation-triangle"
                };
            }
        }
    }

    public class AlertSummaryViewModel
    {
        public int TotalUnhandledAlerts { get; set; }
        public int CriticalAlertCount { get; set; }
        public int WarningAlertCount { get; set; }
        public int InfoAlertCount { get; set; }
    }

    public class CriticalAlertsViewModel
    {
        public List<CriticalAlertViewModel> Alerts { get; set; }
        public AlertSummaryViewModel Summary { get; set; }
    }
}