using CleanroomMonitoring.Web.Models;
using System.Security.Claims;

namespace CleanroomMonitoring.Web.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Kiểm tra đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<(bool success, string message)> ValidateUserAsync(string username, string password);

        /// <summary>
        /// Tạo ClaimsPrincipal (dùng cho đăng nhập cookie)
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(string username);

        /// <summary>
        ///  Mã hóa mật khẩu
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        string HashPassword(string password);

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        Task<bool> RegisterUserAsync(User user, string password, string roleName = "User");

        /// <summary>
        /// Lấy danh sách quyền của người dùng
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<List<string>> GetUserRolesAsync(string username);
    }
}
