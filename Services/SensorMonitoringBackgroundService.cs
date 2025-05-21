using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.Services
{
    public class SensorMonitoringBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly ILogger<SensorMonitoringBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

        public SensorMonitoringBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<SensorHub> hubContext,
            ILogger<SensorMonitoringBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sensor Monitoring Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    await ProcessSensorDataAsync();
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Error occurred while processing sensor data.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Sensor Monitoring Background Service is stopping.");
        }

        private async Task ProcessSensorDataAsync()
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<dbDataContext>();

                // Get all active rooms
                var rooms = await dbContext.CleanRooms.ToListAsync();

                foreach (var room in rooms) {
                    // Get the latest readings for all sensors in this room
                    var latestReadings = await dbContext.SensorInfos
                        .Where(s => s.RoomID == room.RoomID && s.IsActive == true)
                        .Select(s => new {
                            s.SensorInfoID,
                            LatestReading = s.SensorReadings
                                .OrderByDescending(sr => sr.ReadingTime)
                                .FirstOrDefault(),
                            s.SensorType.TypeName,
                            s.SensorType.Unit
                        })
                        .ToListAsync();

                    if (latestReadings.Any()) {
                        var readings = latestReadings.Select(r => new {
                            SensorID = r.SensorInfoID,
                            Value = r.LatestReading?.ReadingValue,
                            Timestamp = r.LatestReading?.ReadingTime,
                            IsValid = r.LatestReading?.IsValid,
                            TypeName = r.TypeName,
                            Unit = r.Unit
                        }).ToList();

                        // Send update to clients viewing this room
                        await _hubContext.Clients.Group($"Room_{room.RoomID}")
                            .SendAsync("ReceiveSensorUpdates", readings);

                        // Check for any alerts
                        var alerts = readings.Where(r => r.IsValid == false).ToList();
                        if (alerts.Any()) {
                            await _hubContext.Clients.All.SendAsync("ReceiveAlerts", new {
                                RoomID = room.RoomID,
                                RoomName = room.RoomName,
                                Alerts = alerts
                            });
                        }
                    }
                }
            }
        }
    }
}