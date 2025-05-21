using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.ViewComponents
{
    public class SensorDetailsViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorDetailsViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? sensorInfoId, TimeRange timeRange = TimeRange.FourHours, DateTime? startDate = null, DateTime? endDate = null)
        {
            var model = new SensorViewModel3 {
                SensorInfoID = sensorInfoId,
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

            if (sensorInfoId.HasValue) {
                // Lấy thông tin chi tiết của sensor
                model.SensorInfo = await _context.SensorInfos
                    .Include(s => s.SensorType)
                    .Include(s => s.CleanRoom)
                    .Include(s => s.SensorLocations)
                    .FirstOrDefaultAsync(s => s.SensorInfoID == sensorInfoId);

                if (model.SensorInfo != null) {
                    // Xác định khoảng thời gian dựa trên lựa chọn
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

                    // Lấy dữ liệu đọc từ sensor trong khoảng thời gian
                    model.SensorReadings = await _context.SensorReadings
                        .Where(r => r.SensorInfoID == sensorInfoId &&
                                   r.ReadingTime >= startDateTime &&
                                   r.ReadingTime <= endDateTime
                                    //&& r.IsValid == true
                                    )
                        .OrderByDescending(r => r.ReadingTime)
                        .ToListAsync();
                }
            }

            return View(model);
        }
    }
}