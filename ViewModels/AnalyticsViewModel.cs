using CleanroomMonitoring.Web.Services;
using CleanroomMonitoring.Web.ViewModels;

namespace CleanroomMonitoring.Web.ViewModels
{
    /// <summary>
    /// Class cho việc hiển thị thống kê Sensor
    /// </summary>
    public class AnalyticsViewModel
    {
        /// <summary>
        /// Class cho việc hiển thị thống kê Sensor: List CleanRoomViewModel, List SensorViewModel, Dictionary SensorStatistics, Dictionary AlertEvent, Dictionary ChartDataPoint, SelectedRoomId, SelectedRoomName, StartDate, EndDate, UptimePercentage
        /// </summary>
        public AnalyticsViewModel()
        {
            CleanRooms = new List<CleanRoomViewModel>();
            Sensors = new List<SensorViewModel>();
            SensorStatistics = new Dictionary<int, SensorStatistics>();
            AlertEvents = new Dictionary<int, List<AlertEvent>>();
            DailyAverages = new Dictionary<string, List<ChartDataPoint>>();
        }

        // Room selection and filtering options

        /// <summary>
        /// Thông tin CleanRooms: List<CleanRoomViewModel>
        /// </summary>
        public List<CleanRoomViewModel> CleanRooms { get; set; }
        public int? SelectedRoomId { get; set; }
        public string SelectedRoomName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Room data for selected room
        public List<SensorViewModel> Sensors { get; set; }
        public Dictionary<int, SensorStatistics> SensorStatistics { get; set; }
        public Dictionary<int, List<AlertEvent>> AlertEvents { get; set; }
        public Dictionary<string, List<ChartDataPoint>> DailyAverages { get; set; }
        public decimal UptimePercentage { get; set; }
    }
}
