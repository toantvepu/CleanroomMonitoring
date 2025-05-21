namespace CleanroomMonitoring.Web.ViewModels
{
    /// <summary>
    /// Hiển thị thông tin của phòng CleanRoom
    /// </summary>
    public class CleanRoomViewModel
    {
        public int RoomID { get; set; }
        public string? RoomName { get; set; }
        public string? CleanRoomClass { get; set; }
        /// <summary>
        /// Số lượng sensor theo phòng
        /// </summary>
        public int SensorCount { get; set; }
        /// <summary>
        /// Có cảnh báo hay không
        /// </summary>
        public bool HasAlerts { get; set; }
    }
}
