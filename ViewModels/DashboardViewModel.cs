namespace CleanroomMonitoring.Web.ViewModels
{
    public class DashboardViewModel
    {
        public List<CleanRoomViewModel> CleanRooms { get; set; } = new List<CleanRoomViewModel>();
        public CleanRoomDetailsViewModel CurrentRoom { get; set; }
        public Dictionary<string, List<ChartDataPoint>> ChartData { get; set; } = new Dictionary<string, List<ChartDataPoint>>();
        public Dictionary<int, SensorStatusViewModel> SensorStatuses { get; set; } = new Dictionary<int, SensorStatusViewModel>();
    }
 
}