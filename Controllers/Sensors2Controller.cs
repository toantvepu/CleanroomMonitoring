using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using OfficeOpenXml;

using System;
using System.Drawing;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.Controllers
{
    public class Sensors2Controller : Controller
    {
        private readonly dbDataContext _context;

        public Sensors2Controller(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id, TimeRange timeRange = TimeRange.FourHours, DateTime? startDate = null, DateTime? endDate = null)
        {
            var model = new SensorViewModel3 {
                SensorInfoID = id,
                SelectedTimeRange = timeRange,
                StartDate = startDate,
                EndDate = endDate,
                SensorList = await _context.SensorInfos
                    .Include(s => s.SensorType)
                    .Include(s => s.CleanRoom)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SensorName)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorData(int sensorId, TimeRange timeRange = TimeRange.FourHours, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime endDateTime = endDate ?? DateTime.Now;
            DateTime startDateTime;

            switch (timeRange) {
                case TimeRange.FourHours:
                    startDateTime = endDateTime.AddHours(-4);
                    break;
                case TimeRange.EightHours:
                    startDateTime = endDateTime.AddHours(-8);
                    break;
                case TimeRange.TwentyFourHours:
                    startDateTime = endDateTime.AddHours(-24);
                    break;
                case TimeRange.Custom:
                    startDateTime = startDate ?? endDateTime.AddHours(-4);
                    break;
                default:
                    startDateTime = endDateTime.AddHours(-4);
                    break;
            }

            var readings = await _context.SensorReadings
                .Where(r => r.SensorInfoID == sensorId &&
                           r.ReadingTime >= startDateTime &&
                           r.ReadingTime <= endDateTime &&
                           r.IsValid == true)
                .OrderBy(r => r.ReadingTime)
                .Select(r => new {
                    time = r.ReadingTime,
                    value = r.ReadingValue
                })
                .ToListAsync();

            return Json(readings);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int sensorId, TimeRange timeRange = TimeRange.FourHours, DateTime? startDate = null, DateTime? endDate = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // hoặc Commercial tùy theo giấy phép của bạn

            // Xác định khoảng thời gian
            DateTime endDateTime = endDate ?? DateTime.Now;
            DateTime startDateTime;

            switch (timeRange) {
                case TimeRange.FourHours:
                    startDateTime = endDateTime.AddHours(-4);
                    break;
                case TimeRange.EightHours:
                    startDateTime = endDateTime.AddHours(-8);
                    break;
                case TimeRange.TwentyFourHours:
                    startDateTime = endDateTime.AddHours(-24);
                    break;
                case TimeRange.Custom:
                    startDateTime = startDate ?? endDateTime.AddHours(-4);
                    break;
                default:
                    startDateTime = endDateTime.AddHours(-4);
                    break;
            }

            // Lấy thông tin sensor
            var sensor = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .Include(s => s.SensorLocations)
                .FirstOrDefaultAsync(s => s.SensorInfoID == sensorId);

            if (sensor == null) {
                return NotFound("Không tìm thấy sensor");
            }

            // Lấy dữ liệu đọc từ sensor trong khoảng thời gian
            var readings = await _context.SensorReadings
                .Where(r => r.SensorInfoID == sensorId &&
                           r.ReadingTime >= startDateTime &&
                           r.ReadingTime <= endDateTime &&
                           r.IsValid == true)
                .OrderBy(r => r.ReadingTime)
                .ToListAsync();

            using var package = new ExcelPackage();

            // Tạo worksheet cho thông tin sensor
            var infoWorksheet = package.Workbook.Worksheets.Add("Thông tin Sensor");

            // Tiêu đề báo cáo
            infoWorksheet.Cells[1, 1].Value = $"THÔNG TIN CHI TIẾT SENSOR: {sensor.SensorName}";
            infoWorksheet.Cells[1, 1, 1, 4].Merge = true;
            infoWorksheet.Cells[1, 1].Style.Font.Size = 16;
            infoWorksheet.Cells[1, 1].Style.Font.Bold = true;
            infoWorksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            infoWorksheet.Cells[2, 1].Value = $"Khoảng thời gian: {startDateTime:dd/MM/yyyy HH:mm:ss} - {endDateTime:dd/MM/yyyy HH:mm:ss}";
            infoWorksheet.Cells[2, 1, 2, 4].Merge = true;
            infoWorksheet.Cells[2, 1].Style.Font.Size = 12;
            infoWorksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Thông tin cơ bản
            int row = 4;
            infoWorksheet.Cells[row, 1].Value = "THÔNG TIN CƠ BẢN";
            infoWorksheet.Cells[row, 1, row, 4].Merge = true;
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;
            infoWorksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            infoWorksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            row++;
            infoWorksheet.Cells[row, 1].Value = "Tên Sensor:";
            infoWorksheet.Cells[row, 2].Value = sensor.SensorName;
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Loại Sensor:";
            infoWorksheet.Cells[row, 2].Value = sensor.SensorType?.TypeName ?? "Không xác định";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Đơn vị:";
            infoWorksheet.Cells[row, 2].Value = sensor.SensorType?.Unit ?? "";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Phòng:";
            infoWorksheet.Cells[row, 2].Value = sensor.CleanRoom?.RoomName ?? "Không xác định";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Vùng:";
            infoWorksheet.Cells[row, 2].Value = sensor.CleanRoom?.Area ?? "Không xác định";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            // Thông tin kỹ thuật
            row += 2;
            infoWorksheet.Cells[row, 1].Value = "THÔNG TIN KỸ THUẬT";
            infoWorksheet.Cells[row, 1, row, 4].Merge = true;
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;
            infoWorksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            infoWorksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            row++;
            infoWorksheet.Cells[row, 1].Value = "Trạng thái:";
            infoWorksheet.Cells[row, 2].Value = sensor.IsActive ? "Hoạt động" : "Không hoạt động";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Địa chỉ IP:";
            infoWorksheet.Cells[row, 2].Value = sensor.IpAddress ?? "Không xác định";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Địa chỉ Modbus:";
            infoWorksheet.Cells[row, 2].Value = sensor.ModbusAddress?.ToString() ?? "Không xác định";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Phase:";
            infoWorksheet.Cells[row, 2].Value = sensor.Phase ?? "Không xác định";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            infoWorksheet.Cells[row, 1].Value = "Ghi chú:";
            infoWorksheet.Cells[row, 2].Value = sensor.COMMENT ?? "Không có";
            infoWorksheet.Cells[row, 1].Style.Font.Bold = true;

            // Định dạng cột
            infoWorksheet.Column(1).Width = 20;
            infoWorksheet.Column(2).Width = 40;
            infoWorksheet.Column(3).Width = 20;
            infoWorksheet.Column(4).Width = 20;

            // Tạo worksheet cho dữ liệu dạng bảng
            var dataWorksheet = package.Workbook.Worksheets.Add("Dữ liệu");

            // Tiêu đề bảng dữ liệu
            dataWorksheet.Cells[1, 1].Value = $"DỮ LIỆU CHI TIẾT SENSOR: {sensor.SensorName}";
            dataWorksheet.Cells[1, 1, 1, 3].Merge = true;
            dataWorksheet.Cells[1, 1].Style.Font.Size = 14;
            dataWorksheet.Cells[1, 1].Style.Font.Bold = true;
            dataWorksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            dataWorksheet.Cells[2, 1].Value = $"Khoảng thời gian: {startDateTime:dd/MM/yyyy HH:mm:ss} - {endDateTime:dd/MM/yyyy HH:mm:ss}";
            dataWorksheet.Cells[2, 1, 2, 3].Merge = true;
            dataWorksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Header của bảng dữ liệu
            dataWorksheet.Cells[4, 1].Value = "Thời gian";
            dataWorksheet.Cells[4, 2].Value = $"Giá trị ({sensor.SensorType?.Unit ?? ""})";
            dataWorksheet.Cells[4, 3].Value = "Trạng thái";

            var headerRange = dataWorksheet.Cells[4, 1, 4, 3];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Dữ liệu chi tiết
            int dataRow = 5;
            foreach (var reading in readings) {
                dataWorksheet.Cells[dataRow, 1].Value = reading.ReadingTime;
                dataWorksheet.Cells[dataRow, 1].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                dataWorksheet.Cells[dataRow, 2].Value = reading.ReadingValue;

                dataWorksheet.Cells[dataRow, 3].Value = reading.IsValid == true ? "Hợp lệ" : "Không hợp lệ";

                dataRow++;
            }

            // Định dạng cột
            dataWorksheet.Column(1).Width = 25;
            dataWorksheet.Column(2).Width = 20;
            dataWorksheet.Column(3).Width = 15;

            // Tạo worksheet cho đồ thị
            // Worksheet Đồ thị
            if (readings.Any()) {
                var chartWorksheet = package.Workbook.Worksheets.Add("Đồ thị");

                // Tiêu đề cột
                chartWorksheet.Cells[1, 1].Value = "Thời gian";
                chartWorksheet.Cells[1, 2].Value = $"Giá trị ({sensor.SensorType?.Unit ?? ""})";

                int chartDataRow = 2;
                foreach (var reading in readings) {
                    // Xử lý ReadingTime nullable
                    chartWorksheet.Cells[chartDataRow, 1].Value =
                        reading.ReadingTime.HasValue ? reading.ReadingTime.Value.ToOADate() : (object)null;
                    chartWorksheet.Cells[chartDataRow, 1].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";

                    chartWorksheet.Cells[chartDataRow, 2].Value = reading.ReadingValue;
                    chartDataRow++;
                }

                // Tạo biểu đồ
                //var chart = chartWorksheet.Drawings.AddChart("SensorChart", eChartType.Line) as ExcelLineChart;
                var chart = chartWorksheet.Drawings.AddChart("chart", eChartType.XYScatterSmooth);
                chart.Title.Text = $"Biểu đồ dữ liệu sensor: {sensor.SensorName}";
                chart.SetPosition(1, 0, 5, 0);
                chart.SetSize(1600, 600);

                var series = chart.Series.Add(
                    chartWorksheet.Cells[2, 2, chartDataRow - 1, 2],
                    chartWorksheet.Cells[2, 1, chartDataRow - 1, 1]
                );
                series.Header = sensor.SensorName;

                // Hiển thị giá trị dữ liệu trên biểu đồ
                if (series is ExcelLineChartSerie lineSerie) {
                    lineSerie.DataLabel.ShowValue = true;
                }

                // Định dạng trục
                chart.XAxis.Title.Text = "Thời gian";
                chart.YAxis.Title.Text = sensor.SensorType?.Unit ?? "";
                chart.XAxis.MajorTickMark = eAxisTickMark.Cross;
                chart.YAxis.MajorTickMark = eAxisTickMark.Cross;

                // Bổ sung định dạng thời gian trục X
                chart.XAxis.Format = "dd/MM/yyyy HH:mm:ss";
                //chart.XAxis.NumberFormat = "dd/MM/yyyy HH:mm:ss";
                //chart.XAxis.IsDateTimeAxis = true; // QUAN TRỌNG: buộc trục X là kiểu thời gian
                chart.XAxis.TickLabelPosition = eTickLabelPosition.Low;
                chart.XAxis.CrossBetween = eCrossBetween.MidCat;

                // Định dạng cột
                chartWorksheet.Column(1).Width = 25;
                chartWorksheet.Column(2).Width = 15;
            }

            // Xuất file Excel
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"Sensor_{sensor.SensorName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}