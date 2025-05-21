using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;

using Humanizer;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.Controllers
{
    public class SensorsController : Controller
    {
        private readonly dbDataContext _context;

        public SensorsController(dbDataContext context)
        {
            _context = context;
        }

        // Action principale - affiche la page avec ou sans capteur sélectionné
        public async Task<IActionResult> Index(int? sensorInfoId)
        {
            // Récupérer tous les capteurs actifs pour le dropdown
            var availableSensors = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .Where(s => s.IsActive)
                .OrderBy(s => s.SensorName)
                .ToListAsync();

            // Si aucun capteur n'est spécifié et qu'il y a des capteurs disponibles, prendre le premier
            if (sensorInfoId == null && availableSensors.Any()) {
                sensorInfoId = availableSensors.First().SensorInfoID;
            }

            // Préparer le ViewModel
            var viewModel = new SensorViewModel2 {
                AvailableSensors = availableSensors,
                TimeRange = "4h",
                EndDate = DateTime.Now,
                StartDate = DateTime.Now.AddHours(-4)
            };

            // Si un capteur est sélectionné, charger ses données
            if (sensorInfoId.HasValue) {
                await LoadSensorData(viewModel, sensorInfoId.Value);
            }

            return View(viewModel);
        }

        // Action pour le chargement des données d'un capteur via AJAX
        [HttpGet]
        public async Task<IActionResult> GetSensorData(int sensorInfoId, string timeRange = "4h",
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var viewModel = new SensorViewModel2 {
                TimeRange = timeRange,
                EndDate = endDate ?? DateTime.Now
            };

            // Déterminer la période selon le timeRange choisi
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

            await LoadSensorData(viewModel, sensorInfoId);

            return PartialView("_SensorDataPartial", viewModel);
        }

        // Action pour récupérer les données de graphique au format JSON
        [HttpGet]
        public async Task<IActionResult> GetChartData(int sensorInfoId, string timeRange = "4h",
            DateTime? startDate = null, DateTime? endDate = null)
        {
            // Déterminer les dates
            DateTime end = endDate ?? DateTime.Now;
            DateTime start;

            if (startDate.HasValue) {
                start = startDate.Value;
            }
            else {
                switch (timeRange) {
                    case "4h":
                        start = end.AddHours(-4);
                        break;
                    case "8h":
                        start = end.AddHours(-8);
                        break;
                    case "24h":
                        start = end.AddHours(-24);
                        break;
                    default:
                        start = end.AddHours(-4);
                        break;
                }
            }

            // Récupérer les données du capteur
            var sensor = await _context.SensorInfos
                .Include(s => s.SensorType)
                .FirstOrDefaultAsync(s => s.SensorInfoID == sensorInfoId);

            if (sensor == null) {
                return NotFound();
            }

            // Récupérer les lectures pour la période spécifiée
            var readings = await _context.SensorReadings
                .Where(r => r.SensorInfoID == sensorInfoId
                         && r.ReadingTime >= start
                         && r.ReadingTime <= end
                         && r.IsValid == true)
                .OrderBy(r => r.ReadingTime)
                .ToListAsync();

            // Créer le modèle de données pour le graphique
            var chartData = new ChartDataViewModel {
                SensorName = sensor.SensorName,
                Unit = sensor.SensorType?.Unit ?? ""
            };

            foreach (var reading in readings) {
                if (reading.ReadingTime.HasValue && reading.ReadingValue.HasValue) {
                    chartData.Labels.Add(reading.ReadingTime.Value.ToString("HH:mm:ss"));
                    chartData.Values.Add(reading.ReadingValue.Value);
                }
            }

            return Json(chartData);
        }

        // Private method to load data from a sensor
        private async Task LoadSensorData(SensorViewModel2 viewModel, int sensorInfoId)
        {
            // Load sensor information
            viewModel.SensorInfo = await _context.SensorInfos
                .Include(s => s.SensorType)
                .Include(s => s.CleanRoom)
                .FirstOrDefaultAsync(s => s.SensorInfoID == sensorInfoId);

            if (viewModel.SensorInfo == null) {
                return;
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

            // Références additionnelles pour faciliter l'accès
            viewModel.CleanRoom = viewModel.SensorInfo.CleanRoom;
            viewModel.SensorType = viewModel.SensorInfo.SensorType;
        }
    }
}