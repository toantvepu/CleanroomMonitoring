using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace CleanroomMonitoring.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly dbDataContext _context;

        public AuthService(dbDataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kiểm tra tài khoản, mật khẩu khi đăng nhập.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<(bool success, string message)> ValidateUserAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return (false, "User not found");

            if (user.IsActive == false)
                return (false, "Account is disabled");

            // Verify password. // Mã hóa mật khẩu nhập vào để so sánh với mật khẩu đã lưu
            string hashedPassword = HashPassword(password);

            // Using PasswordHash field for comparison
            if (user.PasswordHash != hashedPassword) {
                // Update failed login attempts  // Nếu sai, tăng số lần nhập sai
                user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

                // If too many failed attempts, lock the account // Nếu nhập sai quá 5 lần, khóa tài khoản
                if (user.FailedLoginAttempts >= 5) {
                    user.IsActive = false;
                }

                await _context.SaveChangesAsync();
                return (false, "Invalid password");
            }

            // Reset failed login attempts on successful login  // Nếu đúng, reset số lần nhập sai
            user.FailedLoginAttempts = 0;
            user.LastPasswordChange = user.LastPasswordChange ?? DateTime.Now;
            await _context.SaveChangesAsync();

            return (true, "Authentication successful");
        }

        /// <summary>
        /// Tạo thông tin xác thực cho người dùng Tạo ClaimsPrincipal (dùng cho đăng nhập cookie)
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                throw new ArgumentException("User not found");

            var roles = await GetUserRolesAsync(username);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            // Add roles as claims  // Thêm các role vào claims
            foreach (var role in roles) {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }

        /// <summary>
        /// Mã hóa mật khẩu (bảo mật).
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create()) {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<bool> RegisterUserAsync(User user, string password, string roleName = "User")
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return false;

            // Hash the password
            user.PasswordHash = HashPassword(password);
            user.Password = null; // Don't store plain text password
            user.IsActive = true;
            user.FailedLoginAttempts = 0;
            user.LastPasswordChange = DateTime.Now;
            user.PasswordExpiryDate = DateTime.Now.AddDays(90); // 90-day password policy

            // Start a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try {
                // Add user
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Get role ID
                var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null) {
                    // Create default role if it doesn't exist
                    role = new UserRole { RoleName = roleName, Description = $"Default {roleName} role" };
                    _context.UserRoles.Add(role);
                    await _context.SaveChangesAsync();
                }

                // Assign role to user
                var userRoleMapping = new UserRoleMapping {
                    UserID = user.UserID,
                    RoleID = role.RoleID,
                    AssignedDate = DateTime.Now
                };

                _context.UserRoleMappings.Add(userRoleMapping);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách vai trò của người dùng.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<List<string>> GetUserRolesAsync(string username)
        {
            return await _context.Users
                .Where(u => u.Username == username)
                .SelectMany(u => u.UserRoleMappings
                    .Select(rm => rm.UserRole.RoleName))
                .ToListAsync();
        }
    }
}
