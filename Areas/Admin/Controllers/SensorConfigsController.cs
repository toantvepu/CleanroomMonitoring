using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace CleanroomMonitoring.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SensorConfigsController : Controller
    {
        private readonly dbDataContext _context;

        public SensorConfigsController(dbDataContext context)
        {
            _context = context;
        }

        // GET: Admin/SensorConfigs/Create
        public async Task<IActionResult> Create(int sensorId)
        {
            // Check if sensor exists
            var sensor = await _context.SensorInfos.FindAsync(sensorId);
            if (sensor == null) {
                TempData["ErrorMessage"] = "Sensor not found.";
                return RedirectToAction("Index", "SensorInfos");
            }

            // Check if config already exists
            var existingConfig = await _context.SensorConfigs
                .FirstOrDefaultAsync(c => c.SensorInfoID == sensorId);

            if (existingConfig != null) {
                TempData["ErrorMessage"] = "Configuration already exists for this sensor.";
                return RedirectToAction("Details", "SensorInfos", new { id = sensorId });
            }

            // Set default values for new configuration
            var sensorConfig = new SensorConfig {
                SensorInfoID = sensorId,
                IsMonitored = true,
                RequestConvertData = false,
                ScanInterval = 60 // Default to 60 seconds
            };

            // Pass sensor info for reference
            ViewBag.SensorInfo = sensor;

            return View(sensorConfig);
        }

        // POST: Admin/SensorConfigs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SensorInfoID,MinValidValue,MaxValidValue,ScanInterval,IsMonitored,RequestConvertData,LowAlertThreshold,HighAlertThreshold")] SensorConfig sensorConfig)
        {
            if (ModelState.IsValid) {
                _context.Add(sensorConfig);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sensor configuration created successfully.";
                return RedirectToAction("Details", "SensorInfos", new { id = sensorConfig.SensorInfoID });
            }

            // If there's an error, retrieve sensor info again
            var sensor = await _context.SensorInfos.FindAsync(sensorConfig.SensorInfoID);
            ViewBag.SensorInfo = sensor;

            return View(sensorConfig);
        }

        // GET: Admin/SensorConfigs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var sensorConfig = await _context.SensorConfigs
                .Include(s => s.SensorInfo)
                .FirstOrDefaultAsync(m => m.SensorConfigID == id);

            if (sensorConfig == null) {
                return NotFound();
            }

            return View(sensorConfig);
        }

        // POST: Admin/SensorConfigs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SensorConfigID,SensorInfoID,MinValidValue,MaxValidValue,ScanInterval,IsMonitored,RequestConvertData,LowAlertThreshold,HighAlertThreshold")] SensorConfig sensorConfig)
        {
            if (id != sensorConfig.SensorConfigID) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(sensorConfig);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sensor configuration updated successfully.";
                }
                catch (DbUpdateConcurrencyException) {
                    if (!SensorConfigExists(sensorConfig.SensorConfigID)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction("Details", "SensorInfos", new { id = sensorConfig.SensorInfoID });
            }
            return View(sensorConfig);
        }

        private bool SensorConfigExists(int id)
        {
            return _context.SensorConfigs.Any(e => e.SensorConfigID == id);
        }
    }
}