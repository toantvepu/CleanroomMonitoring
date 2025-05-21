using CleanroomMonitoring.Web.Models;

namespace CleanroomMonitoring.Web.ViewModels
{
    public class SensorStatusViewModel2
    {
        public int TotalSensors { get; set; }
        public int ConnectedSensors { get; set; }
        public int DisconnectedSensors { get; set; }
        public List<SensorConnectionStatus> SensorStatuses { get; set; }
    }
}
