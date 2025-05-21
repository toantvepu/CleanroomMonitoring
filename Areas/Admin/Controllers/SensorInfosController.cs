using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace CleanroomMonitoring.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class SensorInfosController : Controller
	{
		private readonly dbDataContext _context;

		public SensorInfosController(dbDataContext context)
		{
			_context = context;
		}

        // GET: Admin/SensorInfos
        // GET: Admin/SensorInfos
        public async Task<IActionResult> Index(string searchName, string searchRoom, string sensorType, string phase)
        {
            // Store current filter parameters to restore them in the view
            ViewBag.CurrentSearchName = searchName;
            ViewBag.CurrentSearchRoom = searchRoom;
            ViewBag.CurrentSensorType = sensorType;
            ViewBag.CurrentPhase = phase; // This matches the view now

            // Prepare sensor type filter list with the 3 specified types
            ViewBag.SensorTypes = new SelectList(new List<string> { "Pressure", "Humidity", "Temperature" });

            // Prepare area filter list with the specified areas
            ViewBag.Phases = new SelectList(new List<string> { "2FP1", "3FP1", "3FP2" });

            // Start with all sensors and apply filters progressively
            var query = _context.SensorInfos
                .Include(s => s.CleanRoom)
                .Include(s => s.SensorType)
                .AsQueryable();

            // Apply Sensor Name filter if provided
            if (!string.IsNullOrEmpty(searchName)) {
                query = query.Where(s => s.SensorName.Contains(searchName));
            }

            // Apply Clean Room filter if provided
            if (!string.IsNullOrEmpty(searchRoom)) {
                query = query.Where(s => s.CleanRoom.RoomName.Contains(searchRoom));
            }

            // Apply Sensor Type filter if provided
            if (!string.IsNullOrEmpty(sensorType)) {
                query = query.Where(s => s.SensorType.TypeName == sensorType);
            }

            // Apply Phase filter if provided
            if (!string.IsNullOrEmpty(phase)) {
                query = query.Where(s => s.Phase == phase);
            }

            // Apply final ordering and get results
            var sensorInfos = await query.OrderBy(s => s.SensorName).ToListAsync();

            return View(sensorInfos);
        }

        // GET: Admin/SensorInfos/Details/5
        public async Task<IActionResult> Details(int? id)
		{
			if (id == null) {
				return NotFound();
			}

			var sensorInfo = await _context.SensorInfos
				.Include(s => s.CleanRoom)
				.Include(s => s.SensorType)
				.Include(s => s.SensorReadings)
                .Include(s => s.SensorLocations)
                .FirstOrDefaultAsync(m => m.SensorInfoID == id);

			if (sensorInfo == null) {
				return NotFound();
			}
            // Get associated SensorConfig if exists
            var sensorConfig = await _context.SensorConfigs
                .FirstOrDefaultAsync(c => c.SensorInfoID == id);

            ViewBag.SensorConfig = sensorConfig;
            return View(sensorInfo);
		}

		// GET: Admin/SensorInfos/Create
		public async Task<IActionResult> Create()
		{
			ViewBag.RoomID = new SelectList(await _context.CleanRooms.ToListAsync(), "RoomID", "RoomName");
			ViewBag.SensorTypeID = new SelectList(await _context.SensorTypes.ToListAsync(), "SensorTypeID", "TypeName");

			// Default values for new sensors
			var sensorInfo = new SensorInfo {
				IsActive = true,
				Phase = "A"  // Default phase
			};

			return View(sensorInfo);
		}

		// POST: Admin/SensorInfos/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("SensorName,RoomID,SensorTypeID,ModbusAddress,IpAddress,Phase,IsActive,COMMENT")] SensorInfo sensorInfo)
		{
			if (ModelState.IsValid) {
				_context.Add(sensorInfo);
				await _context.SaveChangesAsync();

				// Create default sensor location if needed
				await CreateDefaultSensorLocation(sensorInfo.SensorInfoID);

				TempData["SuccessMessage"] = "Sensor created successfully.";
				return RedirectToAction(nameof(Index));
			}

			ViewBag.RoomID = new SelectList(await _context.CleanRooms.ToListAsync(), "RoomID", "RoomName", sensorInfo.RoomID);
			ViewBag.SensorTypeID = new SelectList(await _context.SensorTypes.ToListAsync(), "SensorTypeID", "TypeName", sensorInfo.SensorTypeID);
			return View(sensorInfo);
		}

		// GET: Admin/SensorInfos/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) {
				return NotFound();
			}

			var sensorInfo = await _context.SensorInfos.FindAsync(id);
			if (sensorInfo == null) {
				return NotFound();
			}

			ViewBag.RoomID = new SelectList(await _context.CleanRooms.ToListAsync(), "RoomID", "RoomName", sensorInfo.RoomID);
			ViewBag.SensorTypeID = new SelectList(await _context.SensorTypes.ToListAsync(), "SensorTypeID", "TypeName", sensorInfo.SensorTypeID);

			return View(sensorInfo);
		}

		// POST: Admin/SensorInfos/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("SensorInfoID,SensorName,RoomID,SensorTypeID,ModbusAddress,IpAddress,Phase,IsActive,COMMENT")] SensorInfo sensorInfo)
		{
			if (id != sensorInfo.SensorInfoID) {
				return NotFound();
			}

			if (ModelState.IsValid) {
				try {
					_context.Update(sensorInfo);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "Sensor updated successfully.";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException) {
					if (!SensorInfoExists(sensorInfo.SensorInfoID)) {
						return NotFound();
					}
					else {
						throw;
					}
				}
			}

			// Nếu ModelState có lỗi, in ra lỗi để debug
			foreach (var key in ModelState.Keys) {
				var state = ModelState[key];
				foreach (var error in state.Errors) {
					Console.WriteLine($"{key}: {error.ErrorMessage}");
				}
			}

			ViewBag.RoomID = new SelectList(await _context.CleanRooms.ToListAsync(), "RoomID", "RoomName", sensorInfo.RoomID);
			ViewBag.SensorTypeID = new SelectList(await _context.SensorTypes.ToListAsync(), "SensorTypeID", "TypeName", sensorInfo.SensorTypeID);
			return View(sensorInfo);
		}

		// GET: Admin/SensorInfos/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) {
				return NotFound();
			}

			var sensorInfo = await _context.SensorInfos
				.Include(s => s.CleanRoom)
				.Include(s => s.SensorType)
				.Include(s => s.SensorReadings)
				.FirstOrDefaultAsync(m => m.SensorInfoID == id);

			if (sensorInfo == null) {
				return NotFound();
			}

			// Check if this sensor has readings
			if (sensorInfo.SensorReadings != null && sensorInfo.SensorReadings.Any()) {
				TempData["ErrorMessage"] = "This sensor has sensor reading data. Deleting it will also delete all its readings. Use with caution.";
			}

			return View(sensorInfo);
		}

		// POST: Admin/SensorInfos/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			// Include related data for cascade delete handling
			var sensorInfo = await _context.SensorInfos
				.Include(s => s.SensorReadings)
				
				.FirstOrDefaultAsync(m => m.SensorInfoID == id);

			if (sensorInfo == null) {
				return NotFound();
			}

			// Start a transaction for deleting sensor and related data
			using (var transaction = await _context.Database.BeginTransactionAsync()) {
				try {
					// Delete associated sensor locations
					var sensorLocations = await _context.SensorLocations
						.Where(l => l.SensorInfoID == id)
						.ToListAsync();

					if (sensorLocations.Any()) {
						_context.SensorLocations.RemoveRange(sensorLocations);
					}

					// Delete associated readings (if cascade delete is not already configured)
					var sensorReadings = await _context.SensorReadings
						.Where(r => r.SensorInfoID == id)
						.ToListAsync();

					if (sensorReadings.Any()) {
						_context.SensorReadings.RemoveRange(sensorReadings);
					}

					// Delete the sensor itself
					_context.SensorInfos.Remove(sensorInfo);
					await _context.SaveChangesAsync();

					// Commit the transaction
					await transaction.CommitAsync();

					TempData["SuccessMessage"] = "Sensor and all related data deleted successfully.";
				}
				catch (Exception ex) {
					// Rollback the transaction on error
					await transaction.RollbackAsync();
					TempData["ErrorMessage"] = $"Error deleting sensor: {ex.Message}";
				}
			}

			return RedirectToAction(nameof(Index));
		}

		// Helper method to create a default sensor location
		private async Task CreateDefaultSensorLocation(int sensorInfoId)
		{
			// Check if this sensor already has a location
			var existingLocation = await _context.SensorLocations
				.AnyAsync(l => l.SensorInfoID == sensorInfoId);

			if (!existingLocation) {
				var location = new SensorLocation {
					SensorInfoID = sensorInfoId,
					LocationName = "Default Location",
					XCoordinate = "0",
					YCoordinate = "0",
					ZCoordinate = "0",
					IsActive = true,
					CreatedDate = DateTime.Now,
					CreatedByUserID = GetCurrentUserId(),
					ToaDoX = 0,
					ToaDoY = 0,
					OverlayDirection = "N" // North by default
				};

				_context.SensorLocations.Add(location);
				await _context.SaveChangesAsync();
			}
		}

		private bool SensorInfoExists(int id)
		{
			return _context.SensorInfos.Any(e => e.SensorInfoID == id);
		}

		private int? GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
			return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
		}
	}
}