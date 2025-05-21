using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class EnhancedSensorMapViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public EnhancedSensorMapViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string area = "1F", string sensorType = "all")
        {
            // Get floor information
            var floors = new List<FloorInfo>
            {
                new FloorInfo { Name = "1F", DisplayName = "Floor 1", Phases = 2 },
                new FloorInfo { Name = "2F", DisplayName = "Floor 2", Phases = 2 },
                new FloorInfo { Name = "3F", DisplayName = "Floor 3", Phases = 2 }
            };

            // Parse area info to determine floor and phase
            string floorName = "1F";
            int phase = 1;

            if (!string.IsNullOrEmpty(area)) {
                if (area.Contains("P")) {
                    // Format is like "1FP1"
                    var parts = area.Split('P');
                    floorName = parts[0];
                    if (parts.Length > 1 && int.TryParse(parts[1], out int parsedPhase)) {
                        phase = parsedPhase;
                    }
                }
                else {
                    // Format is just floor like "1F"
                    floorName = area;
                }
            }

            // Validate floor name
            if (!floors.Any(f => f.Name == floorName)) {
                floorName = "1F";
            }

            // Get floor info
            var currentFloor = floors.FirstOrDefault(f => f.Name == floorName);

            // Validate phase
            if (phase < 1 || phase > currentFloor.Phases) {
                phase = 1;
            }

            // Get clean rooms for this floor and phase
            var cleanRooms = await _context.CleanRooms
                .Where(r => r.Area != null && r.Area.Contains($"{floorName}P{phase}"))
                .ToListAsync();

            // Get all sensor types for filtering
            var sensorTypes = await _context.SensorTypes.ToListAsync();

            // Get sensors for this floor based on room IDs
            var roomIds = cleanRooms.Select(r => r.RoomID).ToList();

            var sensorsQuery = _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .Include(s => s.SensorLocations)
                .Include(s => s.SensorFlags)
                .Where(s => s.RoomID != null && roomIds.Contains(s.RoomID.Value));

            // Filter by sensor type if specified
            if (!string.IsNullOrEmpty(sensorType) && sensorType.ToLower() != "all") {
                int? sensorTypeId = null;

                // Check if it's a valid sensor type name
                var matchedType = sensorTypes.FirstOrDefault(t =>
                    t.TypeName.ToLower() == sensorType.ToLower());

                if (matchedType != null) {
                    sensorTypeId = matchedType.SensorTypeID;
                    sensorsQuery = sensorsQuery.Where(s => s.SensorTypeID == sensorTypeId);
                }
            }

            // Filter by alerts only if specified
            if (sensorType.ToLower() == "alerts") {
                sensorsQuery = sensorsQuery.Where(s =>
                    s.SensorFlags.Any(f => f.HasAbnormalValue == true));
            }

            var sensors = await sensorsQuery.ToListAsync();

            // Get latest readings for each sensor
            var sensorIds = sensors.Select(s => s.SensorInfoID).ToList();
            var latestReadings = new Dictionary<int, SensorReading>();

            if (sensorIds.Any()) {
                // Get the most recent reading for each sensor
                var readings = await _context.SensorReadings
                    .Where(r => sensorIds.Contains(r.SensorInfoID) && r.IsValid == true)
                    .GroupBy(r => r.SensorInfoID)
                    .Select(g => g.OrderByDescending(r => r.ReadingTime).FirstOrDefault())
                    .ToListAsync();

                foreach (var reading in readings) {
                    if (reading != null) {
                        latestReadings[reading.SensorInfoID] = reading;
                    }
                }
            }

            // Get alert thresholds for each sensor
            var alertThresholds = await _context.AlertThresholds
                .Where(t => sensorIds.Contains(t.SensorInfoID))
                .ToListAsync();

            // Get recent alerts
            var recentAlerts = await _context.AlertHistorys
                .Where(a => sensorIds.Contains(a.SensorInfoID) &&
                            (a.IsHandled == false || a.IsHandled == null) &&
                            a.AlertTime > DateTime.Now.AddDays(-1))
                .ToListAsync();

            // Create sensor map view models
            var sensorMapItems = new List<SensorMapItemViewModel>();

            foreach (var sensor in sensors) {
                // Get thresholds for this sensor
                var threshold = alertThresholds
                    .FirstOrDefault(t => t.SensorInfoID == sensor.SensorInfoID);

                // Get latest reading
                latestReadings.TryGetValue(sensor.SensorInfoID, out var latestReading);

                // Check if sensor has active alerts
                var hasAlert = recentAlerts.Any(a => a.SensorInfoID == sensor.SensorInfoID);

                // Determine status class
                string statusClass = "secondary"; // Default

                if (latestReading != null && latestReading.ReadingValue.HasValue && threshold != null) {
                    var value = latestReading.ReadingValue.Value;

                    if (threshold.MinValue.HasValue && value < threshold.MinValue.Value ||
                        threshold.MaxValue.HasValue && value > threshold.MaxValue.Value) {
                        statusClass = "danger";
                    }
                    else if (threshold.WarningMinValue.HasValue && value < threshold.WarningMinValue.Value ||
                             threshold.WarningMaxValue.HasValue && value > threshold.WarningMaxValue.Value) {
                        statusClass = "warning";
                    }
                    else {
                        statusClass = "success";
                    }
                }

                // Override status if sensor has flags
                if (sensor.SensorFlags != null && sensor.SensorFlags.Any()) {
                    var flag = sensor.SensorFlags.FirstOrDefault();
                    if (flag != null && flag.HasAbnormalValue == true) {
                        statusClass = "danger";
                    }
                }

                // Create map item view model
                var mapItem = new SensorMapItemViewModel {
                    SensorId = sensor.SensorInfoID,
                    SensorName = sensor.SensorName,
                    RoomId = sensor.RoomID ?? 0,
                    RoomName = sensor.CleanRoom?.RoomName ?? "Unknown",
                    SensorTypeId = sensor.SensorTypeID ?? 0,
                    SensorTypeName = sensor.SensorType?.TypeName ?? "Unknown",
                    SensorTypeUnit = sensor.SensorType?.Unit ?? "",
                    // Fix for CS1061: Replace the incorrect reference to 'SensorLocation' with the correct property or collection from the provided type signatures.

                    XCoordinate = sensor.SensorLocations?.FirstOrDefault()?.ToaDoX ?? 0,
                    YCoordinate = sensor.SensorLocations?.FirstOrDefault()?.ToaDoY ?? 0,
                   
                    StatusClass = statusClass,
                    HasAlert = hasAlert,
                    CurrentValue = latestReading?.ReadingValue,
                    LastUpdated = latestReading?.ReadingTime,
                    IsActive = sensor.IsActive,
                    Phase = sensor.Phase ?? ""
                };

                sensorMapItems.Add(mapItem);
            }

            // Create view model
            var viewModel = new EnhancedSensorMapViewModel {
                CurrentFloor = floorName,
                CurrentPhase = phase,
                Floors = floors,
                SensorTypes = sensorTypes,
                SensorMapItems = sensorMapItems,
                SelectedSensorType = sensorType,
                RoomCount = cleanRooms.Count,
                SensorCount = sensors.Count(),
                AlertCount = sensorMapItems.Count(s => s.HasAlert)
            };

            return View(viewModel);
        }
    }

    public class FloorInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Phases { get; set; }
    }

    public class SensorMapItemViewModel
    {
        public int SensorId { get; set; }
        public string SensorName { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int SensorTypeId { get; set; }
        public string SensorTypeName { get; set; }
        public string SensorTypeUnit { get; set; }
        public int XCoordinate { get; set; }
        public int YCoordinate { get; set; }
        public string StatusClass { get; set; }
        public bool HasAlert { get; set; }
        public decimal? CurrentValue { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsActive { get; set; }
        public string Phase { get; set; }

        // Icon based on sensor type
        public string Icon
        {
            get
            {
                return SensorTypeName?.ToLower() switch {
                    "temperature" => "bi-thermometer-half",
                    "humidity" => "bi-droplet",
                    "pressure" => "bi-speedometer",
                    _ => "bi-graph-up"
                };
            }
        }
    }

    public class EnhancedSensorMapViewModel
    {
        public string CurrentFloor { get; set; }
        public int CurrentPhase { get; set; }
        public List<FloorInfo> Floors { get; set; }
        public List<SensorType> SensorTypes { get; set; }
        public List<SensorMapItemViewModel> SensorMapItems { get; set; }
        public string SelectedSensorType { get; set; }
        public int RoomCount { get; set; }
        public int SensorCount { get; set; }
        public int AlertCount { get; set; }
    }
}