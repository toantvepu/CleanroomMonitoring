namespace CleanroomMonitoring.Web.ViewModels
{
    /// <summary>
    /// Đại diện cho một sự kiện cảnh báo từ cảm biến
    /// </summary>
    public class AlertEvent
    {
        /// <summary>
        /// Thời điểm bắt đầu sự kiện cảnh báo
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Thời điểm kết thúc sự kiện cảnh báo (null nếu vẫn đang diễn ra)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Thời gian kéo dài của sự kiện cảnh báo
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// ID của cảm biến gây ra cảnh báo
        /// </summary>
        public int SensorInfoID { get; set; }

        /// <summary>
        /// Tên của cảm biến gây ra cảnh báo
        /// </summary>
        public string SensorName { get; set; }

        /// <summary>
        /// Loại cảnh báo: "High", "Low", hoặc "Invalid"
        /// </summary>
        public string AlertType { get; set; }

        /// <summary>
        /// Giá trị ngưỡng đã bị vượt quá
        /// </summary>
        public decimal ThresholdValue { get; set; }

        /// <summary>
        /// Giá trị thực tế gây ra cảnh báo
        /// </summary>
        public decimal ActualValue { get; set; }
    }
}
