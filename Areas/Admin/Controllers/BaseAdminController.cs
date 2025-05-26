// ===================================================================
// 🎯 BƯỚC 1: Tạo BaseController để tái sử dụng các hàm chung
// ===================================================================

using CleanroomMonitoring.Web.Controllers;
using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.Services;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using System.Security.Claims;

namespace CleanroomMonitoring.Web.Areas.Admin.Controllers
{
    public abstract class BaseAdminController : Controller
    {
        protected const int DEFAULT_PAGE_SIZE = 10;

        /// <summary>
        /// Lấy ID của user hiện tại
        /// </summary>
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId)) {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        protected void ShowSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        protected void ShowErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Tính toán thông tin phân trang
        /// </summary>
        protected (int totalPages, int currentPage, int startRecord, int endRecord) CalculatePagination(
            int totalRecords, int currentPage, int pageSize = DEFAULT_PAGE_SIZE)
        {
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var startRecord = (currentPage - 1) * pageSize + 1;
            var endRecord = Math.Min(currentPage * pageSize, totalRecords);

            return (totalPages, currentPage, startRecord, endRecord);
        }
    }
}
 