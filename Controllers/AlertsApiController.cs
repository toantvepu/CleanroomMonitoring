using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace CleanroomMonitoring.Web.Controllers
{
    // Add this to an AlertsApiController or create a new one
    [Route("api/Alerts")]
    [ApiController]
    public class AlertsApiController : ControllerBase
    {
        private readonly dbDataContext _context;
        private readonly ILogger<AlertsApiController> _logger;

        public AlertsApiController(dbDataContext context, ILogger<AlertsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("HandleAlert")]
        public async Task<IActionResult> HandleAlert([FromBody] HandleAlertRequest request)
        {
            try {
                var alert = await _context.AlertHistorys.FindAsync(request.AlertId);

                if (alert == null)
                    return NotFound(new { success = false, message = "Alert not found" });

                // Update the alert
                alert.IsHandled = true;

                // Add handling note to comment or create audit trail
                var auditTrail = new AuditTrail {
                    EventTime = DateTime.Now,
                    UserID = User.FindFirstValue("UserID") != null ?
                        int.Parse(User.FindFirstValue("UserID")) : null,
                    EventType = "AlertHandled",
                    TableName = "AlertHistory",
                    RecordID = alert.AlertID.ToString(),
                    OldValue = "IsHandled: False",
                    NewValue = "IsHandled: True",
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ApplicationName = "CleanroomMonitoring",
                    COMMENT = request.Notes
                };

                _context.AuditTrails.Add(auditTrail);

                // Create maintenance event if requested
                if (request.CreateMaintenanceEvent) {
                    var sensor = await _context.SensorInfos.FindAsync(alert.SensorInfoID);

                    if (sensor != null) {
                        var maintenanceEvent = new MaintenanceEvent {
                            RoomID = sensor.RoomID,
                            StartTime = DateTime.Now,
                            EventType = "Alert Response",
                            Description = $"Maintenance event created in response to alert: {alert.AlertMessage}. Notes: {request.Notes}",
                            CreatedByUserID = User.FindFirstValue("UserID") != null ?
                                int.Parse(User.FindFirstValue("UserID")) : null,
                            CreatedDate = DateTime.Now,
                            Status = "Scheduled"
                        };

                        _context.MaintenanceEvents.Add(maintenanceEvent);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error handling alert {AlertId}", request.AlertId);
                return StatusCode(500, new { success = false, message = "An error occurred while handling the alert" });
            }
        }

        public class HandleAlertRequest
        {
            public long AlertId { get; set; }
            public string Notes { get; set; }
            public bool CreateMaintenanceEvent { get; set; }
        }
    }
}
