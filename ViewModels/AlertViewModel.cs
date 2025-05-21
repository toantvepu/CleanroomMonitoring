namespace CleanroomMonitoring.Web.ViewModels
{
    public class AlertViewModel
    {
        public long AlertID { get; set; }
        public string? AlertType { get; set; }
        public string? AlertMessage { get; set; }
        public DateTime? AlertTime { get; set; }
        public string? RoomName { get; set; }
        public string? SensorName { get; set; }
        public decimal? ReadingValue { get; set; }
        public bool IsResolved { get; set; }
    }
}
