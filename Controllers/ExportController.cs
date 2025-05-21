using Microsoft.AspNetCore.Mvc;
using CleanroomMonitoring.Web.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.Controllers
{
    [Route("Export")]
    public class ExportController : Controller
    {
        private readonly dbDataContext _context;

        public ExportController(dbDataContext context)
        {
            _context = context;
        }

        [HttpGet("SensorReadingsFull")]
        public async Task<IActionResult> SensorReadingsFull(string range = "24h", string phase = "ALL")
        {
            DateTime fromTime = range switch {
                "7d" => DateTime.Now.AddDays(-7),
                "30d" => DateTime.Now.AddDays(-30),
                "24h" => DateTime.Now.AddHours(-24),
                "8h" => DateTime.Now.AddHours(-8),
                "2h" => DateTime.Now.AddHours(-2),
                _ => DateTime.Now.AddHours(-24),
            };

            // Lấy dữ liệu cho từng loại
            var tempReadings = await _context.SensorReadings
                .Include(r => r.SensorInfo)
                    .ThenInclude(si => si.CleanRoom)
                .Include(r => r.SensorInfo)
                    .ThenInclude(si => si.SensorType)
                .Where(r => r.ReadingTime >= fromTime
                    && (phase == "ALL" || r.SensorInfo.CleanRoom.Area == phase)
                    && r.SensorInfo.SensorType.TypeName.Contains("Temperature"))
                .OrderBy(r => r.ReadingTime)
                .ToListAsync();

            var humReadings = await _context.SensorReadings
                .Include(r => r.SensorInfo)
                    .ThenInclude(si => si.CleanRoom)
                .Include(r => r.SensorInfo)
                    .ThenInclude(si => si.SensorType)
                .Where(r => r.ReadingTime >= fromTime
                    && (phase == "ALL" || r.SensorInfo.CleanRoom.Area == phase)
                    && r.SensorInfo.SensorType.TypeName.Contains("Humidity"))
                .OrderBy(r => r.ReadingTime)
                .ToListAsync();

            var presReadings = await _context.SensorReadings
                .Include(r => r.SensorInfo)
                    .ThenInclude(si => si.CleanRoom)
                .Include(r => r.SensorInfo)
                    .ThenInclude(si => si.SensorType)
                .Where(r => r.ReadingTime >= fromTime
                    && (phase == "ALL" || r.SensorInfo.CleanRoom.Area == phase)
                    && r.SensorInfo.SensorType.TypeName.Contains("Pressure"))
                .OrderBy(r => r.ReadingTime)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            // Hàm tạo sheet
            void AddSheet(string sheetName, System.Collections.Generic.List<CleanroomMonitoring.Web.Models.SensorReading> readings)
            {
                var ws = package.Workbook.Worksheets.Add(sheetName);
                // Header
                ws.Cells[1, 1].Value = "RoomName";
                ws.Cells[1, 2].Value = "Area";
                ws.Cells[1, 3].Value = "CleanRoomClass";
                ws.Cells[1, 4].Value = "Phase";
                ws.Cells[1, 5].Value = "Point";
                ws.Cells[1, 6].Value = "SensorName";
                ws.Cells[1, 7].Value = "IpAddress";
                ws.Cells[1, 8].Value = "ModbusAddress";
                ws.Cells[1, 9].Value = "ReadingValue";
                ws.Cells[1, 10].Value = "ReadingTime";
                ws.Cells[1, 11].Value = "IsValid";

                using (var range = ws.Cells[1, 1, 1, 11]) {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var r in readings) {
                    var si = r.SensorInfo;
                    var cr = si?.CleanRoom;
                    ws.Cells[row, 1].Value = cr?.RoomName ?? "";
                    ws.Cells[row, 2].Value = cr?.Area ?? "";
                    ws.Cells[row, 3].Value = cr?.CleanRoomClass ?? "";
                    ws.Cells[row, 4].Value = si?.Phase ?? "";
                    ws.Cells[row, 5].Value = si?.COMMENT ?? ""; // Nếu Point là COMMENT, nếu không hãy sửa lại đúng property
                    ws.Cells[row, 6].Value = si?.SensorName ?? "";
                    ws.Cells[row, 7].Value = si?.IpAddress ?? "";
                    ws.Cells[row, 8].Value = si?.ModbusAddress?.ToString() ?? "";
                    ws.Cells[row, 9].Value = r.ReadingValue?.ToString("0.0") ?? "";
                    ws.Cells[row, 10].Value = r.ReadingTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    ws.Cells[row, 11].Value = r.IsValid.HasValue ? (r.IsValid.Value ? "Hợp lệ" : "Lỗi") : "";
                    row++;
                }
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }

            AddSheet("Temperature", tempReadings);
            AddSheet("Humidity", humReadings);
            AddSheet("Pressure", presReadings);

            var fileName = $"SensorReadings_Full_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
