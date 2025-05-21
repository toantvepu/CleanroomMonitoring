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
    public class CleanRoomsController : Controller
    {
        private readonly dbDataContext _context;

        public CleanRoomsController(dbDataContext context)
        {
            _context = context;
        }

        // GET: Admin/CleanRooms
        public async Task<IActionResult> Index()
        {
            var cleanRooms = await _context.CleanRooms
                .Include(c => c.Factory)
                .OrderBy(o=>o.RoomName)
                .ToListAsync();

            return View(cleanRooms);
        }

        // GET: Admin/CleanRooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var cleanRoom = await _context.CleanRooms
                .Include(c => c.Factory)
                .Include(c => c.SensorInfos)
                    .ThenInclude(s => s.SensorType)
                .FirstOrDefaultAsync(m => m.RoomID == id);

            if (cleanRoom == null) {
                return NotFound();
            }

            return View(cleanRoom);
        }

        // GET: Admin/CleanRooms/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.FactoryID = new SelectList(await _context.Factories.ToListAsync(), "FactoryID", "FactoryName");

            ViewBag.CleanRoomClasses  = new List<SelectListItem>
            {
                new SelectListItem { Value = "ISO 1", Text = "ISO 1" },
                new SelectListItem { Value = "ISO 2", Text = "ISO 2" },
                new SelectListItem { Value = "ISO 3", Text = "ISO 3" },
                new SelectListItem { Value = "ISO 4", Text = "ISO 4" },
                new SelectListItem { Value = "ISO 5", Text = "ISO 5" },
                new SelectListItem { Value = "ISO 6", Text = "ISO 6" },
                new SelectListItem { Value = "ISO 7", Text = "ISO 7" },
                new SelectListItem { Value = "ISO 8", Text = "ISO 8" },
                new SelectListItem { Value = "ISO 9", Text = "ISO 9" }
            };

            return View();
        }

        // POST: Admin/CleanRooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomName,Area,Description,FactoryID,CleanRoomClass,COMMENT")] CleanRoom cleanRoom)
        {
            if (ModelState.IsValid) {
                cleanRoom.CreatedDate = DateTime.Now;
                cleanRoom.CreatedByUserID = GetCurrentUserId();

                _context.Add(cleanRoom);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Clean room created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FactoryID = new SelectList(await _context.Factories.ToListAsync(), "FactoryID", "FactoryName", cleanRoom.FactoryID);

            ViewBag.CleanRoomClasses = new List<SelectListItem>
            {
                new SelectListItem { Value = "ISO 1", Text = "ISO 1" },
                new SelectListItem { Value = "ISO 2", Text = "ISO 2" },
                new SelectListItem { Value = "ISO 3", Text = "ISO 3" },
                new SelectListItem { Value = "ISO 4", Text = "ISO 4" },
                new SelectListItem { Value = "ISO 5", Text = "ISO 5" },
                new SelectListItem { Value = "ISO 6", Text = "ISO 6" },
                new SelectListItem { Value = "ISO 7", Text = "ISO 7" },
                new SelectListItem { Value = "ISO 8", Text = "ISO 8" },
                new SelectListItem { Value = "ISO 9", Text = "ISO 9" }
            };

            return View(cleanRoom);
        }

        // GET: Admin/CleanRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var cleanRoom = await _context.CleanRooms.FindAsync(id);
            if (cleanRoom == null) {
                return NotFound();
            }
            ViewBag.FactoryID = new SelectList(await _context.Factories.ToListAsync(), "FactoryID", "FactoryName", cleanRoom.FactoryID);

            ViewBag.CleanRoomClasses = new List<SelectListItem>
            {
                new SelectListItem { Value = "ISO 1", Text = "ISO 1" },
                new SelectListItem { Value = "ISO 2", Text = "ISO 2" },
                new SelectListItem { Value = "ISO 3", Text = "ISO 3" },
                new SelectListItem { Value = "ISO 4", Text = "ISO 4" },
                new SelectListItem { Value = "ISO 5", Text = "ISO 5" },
                new SelectListItem { Value = "ISO 6", Text = "ISO 6" },
                new SelectListItem { Value = "ISO 7", Text = "ISO 7" },
                new SelectListItem { Value = "ISO 8", Text = "ISO 8" },
                new SelectListItem { Value = "ISO 9", Text = "ISO 9" }
            };

            return View(cleanRoom);
        }

        // POST: Admin/CleanRooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomID,RoomName,Area,Description,FactoryID,CleanRoomClass,COMMENT")] CleanRoom cleanRoom)
        {
            if (id != cleanRoom.RoomID) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    cleanRoom.LastModifiedDate = DateTime.Now;
                    cleanRoom.LastModifiedByUserID = GetCurrentUserId();

                    var existingRoom = await _context.CleanRooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomID == id);
                    if (existingRoom != null) {
                        cleanRoom.CreatedDate = existingRoom.CreatedDate;
                        cleanRoom.CreatedByUserID = existingRoom.CreatedByUserID;
                    }

                    _context.Update(cleanRoom);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Clean room updated successfully.";
                }
                catch (DbUpdateConcurrencyException) {
                    if (!CleanRoomExists(cleanRoom.RoomID)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FactoryID  = new SelectList(await _context.Factories.ToListAsync(), "FactoryID", "FactoryName", cleanRoom.FactoryID);

            ViewBag.CleanRoomClasses  = new List<SelectListItem>
            {
                new SelectListItem { Value = "ISO 1", Text = "ISO 1" },
                new SelectListItem { Value = "ISO 2", Text = "ISO 2" },
                new SelectListItem { Value = "ISO 3", Text = "ISO 3" },
                new SelectListItem { Value = "ISO 4", Text = "ISO 4" },
                new SelectListItem { Value = "ISO 5", Text = "ISO 5" },
                new SelectListItem { Value = "ISO 6", Text = "ISO 6" },
                new SelectListItem { Value = "ISO 7", Text = "ISO 7" },
                new SelectListItem { Value = "ISO 8", Text = "ISO 8" },
                new SelectListItem { Value = "ISO 9", Text = "ISO 9" }
            };

            return View(cleanRoom);
        }

        // GET: Admin/CleanRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var cleanRoom = await _context.CleanRooms
                .Include(c => c.Factory)
                .Include(c => c.SensorInfos)
                .FirstOrDefaultAsync(m => m.RoomID == id);

            if (cleanRoom == null) {
                return NotFound();
            }

            // Check if this clean room has associated sensors
            if (cleanRoom.SensorInfos != null && cleanRoom.SensorInfos.Any()) {
                TempData["ErrorMessage"] = "Cannot delete this clean room because it has associated sensors. Please delete or reassign the sensors first.";
                return RedirectToAction(nameof(Index));
            }

            return View(cleanRoom);
        }

        // POST: Admin/CleanRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cleanRoom = await _context.CleanRooms
                .Include(c => c.SensorInfos)
                .FirstOrDefaultAsync(m => m.RoomID == id);

            if (cleanRoom == null) {
                return NotFound();
            }

            // Double-check for associated sensors
            if (cleanRoom.SensorInfos != null && cleanRoom.SensorInfos.Any()) {
                TempData["ErrorMessage"] = "Cannot delete this clean room because it has associated sensors. Please delete or reassign the sensors first.";
                return RedirectToAction(nameof(Index));
            }

            _context.CleanRooms.Remove(cleanRoom);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Clean room deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool CleanRoomExists(int id)
        {
            return _context.CleanRooms.Any(e => e.RoomID == id);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
        }
    }
}