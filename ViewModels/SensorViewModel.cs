namespace CleanroomMonitoring.Web.ViewModels
{
    /// <summary>
    /// Lớp chứa thông tin của sensor,lần đọc cuối, giá trị mới nhất, trạng thái active hay không. có cảnh bảo không
    /// </summary>
    public class SensorViewModel
    {
        public int SensorInfoID { get; set; }
        public string SensorName { get; set; }
        public string SensorTypeName { get; set; }
        public string Unit { get; set; }
        public decimal? LastReading { get; set; }
        public DateTime? LastReadingTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsInAlertState { get; set; }
    }
}
