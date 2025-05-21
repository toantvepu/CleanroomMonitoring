using CleanroomMonitoring.Web.Models;

namespace CleanroomMonitoring.Web.ViewModels
{
    public class SensorViewModel2
    {// Données principales du capteur
        public SensorInfo SensorInfo { get; set; }

        // Informations associées
        public CleanRoom CleanRoom { get; set; }
        public SensorType SensorType { get; set; }
        public List<SensorLocation> SensorLocations { get; set; }

        // Données de lecture
        public List<SensorReading> SensorReadings { get; set; }

        // Paramètres de filtrage
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Options pour l'interface utilisateur
        public string TimeRange { get; set; } = "4h"; // Par défaut 4 heures

        // Liste des capteurs pour le dropdown
        public List<SensorInfo> AvailableSensors { get; set; }
    }

    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Values { get; set; } = new List<decimal>();
        public string SensorName { get; set; }
        public string Unit { get; set; }
    }
}
