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
    public class SensorLocationsController : Controller
    {
        private readonly dbDataContext _context;

        public SensorLocationsController(dbDataContext context)
        {
            _context = context;
        }

        // GET: Admin/SensorLocations/Create
        public async Task<IActionResult> Create(int sensorId)
        {
            // Check if sensor exists
            var sensor = await _context.SensorInfos.FindAsync(sensorId);
            if (sensor == null) {
                TempData["ErrorMessage"] = "Sensor not found.";
                return RedirectToAction("Index", "SensorInfos");
            }

            // Set default values for new location
            var location = new SensorLocation {
                SensorInfoID = sensorId,
                LocationName = $"{sensor.SensorName} Location",
                XCoordinate = "0",
                YCoordinate = "0",
                ZCoordinate = "0",
                ToaDoX = 0,
                ToaDoY = 0,
                OverlayDirection = "top",
              //  OverlayDirection = "N",
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedByUserID = GetCurrentUserId()
            };

            // Pass sensor info for reference
            ViewBag.SensorInfo = sensor;

            return View(location);
        }

        // POST: Admin/SensorLocations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SensorInfoID,LocationName,XCoordinate,YCoordinate,ZCoordinate,ToaDoX,ToaDoY,OverlayDirection,IsActive,COMMENT")] SensorLocation location)
        {
            if (ModelState.IsValid) {
                location.CreatedDate = DateTime.Now;
                location.CreatedByUserID = GetCurrentUserId();

                _context.Add(location);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sensor location created successfully.";
                return RedirectToAction("Details", "SensorInfos", new { id = location.SensorInfoID });
            }

            // If there's an error, retrieve sensor info again
            var sensor = await _context.SensorInfos.FindAsync(location.SensorInfoID);
            ViewBag.SensorInfo = sensor;

            return View(location);
        }

        // GET: Admin/SensorLocations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var location = await _context.SensorLocations
                .Include(l => l.SensorInfo)
                .FirstOrDefaultAsync(m => m.LocationID == id);

            if (location == null) {
                return NotFound();
            }

            return View(location);
        }

        // POST: Admin/SensorLocations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LocationID,SensorInfoID,LocationName,XCoordinate,YCoordinate,ZCoordinate,ToaDoX,ToaDoY,OverlayDirection,IsActive,COMMENT")] SensorLocation location)
        {
            if (id != location.LocationID) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    // Preserve created info, update modified info
                    var existingLocation = await _context.SensorLocations.FindAsync(id);
                    if (existingLocation == null) {
                        return NotFound();
                    }

                    // Keep original creation data
                    location.CreatedDate = existingLocation.CreatedDate;
                    location.CreatedByUserID = existingLocation.CreatedByUserID;

                    // Set modification data
                    location.LastModifiedDate = DateTime.Now;
                    location.LastModifiedByUserID = GetCurrentUserId();

                    _context.Entry(existingLocation).State = EntityState.Detached;
                    _context.Update(location);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Sensor location updated successfully.";
                }
                catch (DbUpdateConcurrencyException) {
                    if (!SensorLocationExists(location.LocationID)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction("Details", "SensorInfos", new { id = location.SensorInfoID });
            }
            return View(location);
        }

        // GET: Admin/SensorLocations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var location = await _context.SensorLocations
                .Include(l => l.SensorInfo)
                .FirstOrDefaultAsync(m => m.LocationID == id);

            if (location == null) {
                return NotFound();
            }

            return View(location);
        }

        // POST: Admin/SensorLocations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.SensorLocations.FindAsync(id);
            if (location == null) {
                return NotFound();
            }

            int sensorId = location.SensorInfoID ?? 0;

            _context.SensorLocations.Remove(location);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sensor location deleted successfully.";
            return RedirectToAction("Details", "SensorInfos", new { id = sensorId });
        }

        private bool SensorLocationExists(int id)
        {
            return _context.SensorLocations.Any(e => e.LocationID == id);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
        }
    }
}