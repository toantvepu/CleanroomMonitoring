
using CleanroomMonitoring;
 
using CleanroomMonitoring.Web;
using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;
 

using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
 
// Thêm Localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Add services to the container.
builder.Services.AddControllersWithViews()
     .AddRazorRuntimeCompilation()  // Tự reload view khi chỉnh sửa .cshtml) 
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64; // Tăng độ sâu tối đa nếu cần
                                                     
        // Cấu hình JSON serialization để xử lý các giá trị decimal chính xác
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Cấu hình Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<dbDataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CleanroomDatabase"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(0)); //Tắt retry
    options.LogTo(Console.WriteLine);
});
// Register DapperHelper
builder.Services.AddScoped<DapperHelper>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("CleanroomDatabase");
    return new DapperHelper(connectionString);
});
// Đăng ký DapperHelper
//builder.Services.AddSingleton<DapperHelper>(sp => new DapperHelper(connectionString));

// Đăng ký IRoomDataService
builder.Services.AddScoped<IRoomDataService, DapperRoomDataService>();
// Đăng ký Memory Cache
builder.Services.AddMemoryCache();
// Add custom services
builder.Services.AddScoped<ISensorDataService, SensorDataService>();
// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Register Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        //  options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Dòng này yêu cầu cookie chỉ được gửi qua HTTPS
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; //không khuyến nghị cho production): Cho phép cookie hoạt động trên HTTP:
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });

// Configure Authorization
builder.Services.AddAuthorization(options => {
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
    options.AddPolicy("RequireAuthenticated", policy => policy.RequireAuthenticatedUser());
});


// Add background service for sensor monitoring
// builder.Services.AddHostedService<SensorMonitoringBackgroundService>();
// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
// Cấu hình cookie đảm bảo rằng cookie không được đánh dấu là Secure.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
  
 
 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true; // Chỉ cho phép truy cập session qua HTTP
    options.Cookie.IsEssential = true; // Đảm bảo cookie session luôn được gửi
});

// Đăng ký các dịch vụ
 
builder.Services.AddSignalR();

var app = builder.Build();

// Cấu hình các ngôn ngữ hỗ trợ
var supportedCultures = new[]
{
    new CultureInfo("en"),  // English
    new CultureInfo("vi"),  // Vietnamese
    new CultureInfo("ja")   // Japanese
};
// Áp dụng các ngôn ngữ mặc định
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Không sử dụng HTTPS Redirection
// app.UseHttpsRedirection(); 

app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); //Đảm bảo API hoạt động bằng cách kích hoạt endpoint

//endpoint configuration
app.UseEndpoints(endpoints => {
    // Admin area routing
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    // Default routing
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    // SensorHub
    endpoints.MapHub<SensorHub>("/sensorHub");

    // ViewComponent routing
    endpoints.MapControllerRoute(
        name: "sensor-chart",
        pattern: "SensorReadingsChart",
        defaults: new { controller = "Components", action = "SensorReadingsChart" }
    );
});

// Thêm middleware xử lý lỗi toàn cục
app.UseMiddleware<GlobalExceptionHandler>();
app.UseCors("AllowAllOrigins");
 
app.Run();
