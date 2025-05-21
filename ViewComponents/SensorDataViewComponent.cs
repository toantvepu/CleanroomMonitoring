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
    public class SensorDataViewComponent : ViewComponent
    {
        private readonly dbDataContext _context;

        public SensorDataViewComponent(dbDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int sensorInfoId, string timeRange = "4h",
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var viewModel = new SensorViewModel2 {
                TimeRange = timeRange,
                EndDate = endDate ?? DateTime.Now
            };

            // Determine the period according to the selected timeRange
            if (startDate.HasValue && endDate.HasValue) {
                viewModel.StartDate = startDate.Value;
                viewModel.EndDate = endDate.Value;
            }
            else {
                switch (timeRange) {
                    case "4h":
                        viewModel.StartDate = viewModel.EndDate.AddHours(-4);
                        break;
                    case "8h":
                        viewModel.StartDate = viewModel.EndDate.AddHours(-8);
                        break;
                    case "24h":
                        viewModel.StartDate = viewModel.EndDate.AddHours(-24);
                        break;
                    default:
                        viewModel.StartDate = viewModel.EndDate.AddHours(-4);
                        break;
                }
            }

            // Load sensor information
            viewModel.SensorInfo = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .FirstOrDefaultAsync(s => s.SensorInfoID == sensorInfoId);

            if (viewModel.SensorInfo == null) {
                return View(viewModel);
            }

            // Charger les emplacements du capteur
            viewModel.SensorLocations = await _context.SensorLocations
                .Where(sl => sl.SensorInfoID == sensorInfoId && sl.IsActive)
                .ToListAsync();

            // Charger les données du capteur pour la période spécifiée
            viewModel.SensorReadings = await _context.SensorReadings
                .Where(r => r.SensorInfoID == sensorInfoId
                         && r.ReadingTime >= viewModel.StartDate
                         && r.ReadingTime <= viewModel.EndDate
                         && r.IsValid == true)
                .OrderByDescending(r => r.ReadingTime)
                .ToListAsync();

            // Additional references to facilitate access
            viewModel.CleanRoom = viewModel.SensorInfo.CleanRoom;
            viewModel.SensorType = viewModel.SensorInfo.SensorType;

            return View(viewModel);
        }
    }
}