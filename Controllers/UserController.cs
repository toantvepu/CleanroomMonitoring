using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace CleanroomMonitoring.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly dbDataContext _context;

        public UserController(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Profile()
        {
            // Lấy ID của người dùng hiện tại từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin user từ database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) {
                return NotFound();
            }

            // Tạo và trả về view model
            var profileViewModel = new UserProfileViewModel {
                UserID = user.UserID,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = user.Department
            };

            return View(profileViewModel);
        }

        public async Task<IActionResult> Settings()
        {
            // Tương tự như Profile, lấy thông tin user để hiển thị/chỉnh sửa
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId)) {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) {
                return NotFound();
            }

            // Tạo và trả về view model cho trang settings
            // (Có thể dùng lại UserProfileViewModel hoặc tạo một ViewModel mới tùy nhu cầu)

            return View(new UserProfileViewModel {
                UserID = user.UserID,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = user.Department
            });
        }
    }
}