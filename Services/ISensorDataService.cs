using CleanroomMonitoring.Web.Models;
using CleanroomMonitoring.Web.ViewModels;

namespace CleanroomMonitoring.Web.Services
{
    /// <summary>
    /// Interface xác định các chức năng để làm việc với dữ liệu từ cảm biến 
    /// trong hệ thống giám sát phòng sạch
    /// </summary>
    public interface ISensorDataService
    {
        /// <summary>
        /// Lấy tất cả các dữ liệu đọc từ một cảm biến trong khoảng thời gian
        /// </summary>
        /// <param name="sensorId">ID của cảm biến</param>
        /// <param name="startTime">Thời gian bắt đầu</param>
        /// <param name="endTime">Thời gian kết thúc</param>
        /// <returns>Danh sách các dữ liệu đọc từ cảm biến</returns>
        Task<IEnumerable<SensorReading>> GetSensorReadingsAsync(int sensorId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Tính toán các thống kê cơ bản cho dữ liệu từ một cảm biến
        /// </summary>
        /// <param name="sensorId">ID của cảm biến</param>
        /// <param name="startTime">Thời gian bắt đầu</param>
        /// <param name="endTime">Thời gian kết thúc</param>
        /// <returns>Từ điển chứa các chỉ số thống kê (min, max, avg, count)</returns>
        Task<Dictionary<string, object>> GetSensorStatisticsDictionaryAsync(int sensorId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Kiểm tra xem cảm biến có đang trong trạng thái cảnh báo không
        /// </summary>
        /// <param name="sensorId">ID của cảm biến</param>
        /// <returns>true nếu cảm biến đang trong trạng thái cảnh báo</returns>
        Task<bool> IsSensorInAlertStateAsync(int sensorId);

        /// <summary>
        /// Cung cấp các thống kê chi tiết của một cảm biến
        /// </summary>
        /// <param name="sensorId">ID của cảm biến</param>
        /// <param name="startDate">Thời gian bắt đầu</param>
        /// <param name="endDate">Thời gian kết thúc</param>
        /// <returns>Đối tượng SensorStatistics chứa các chỉ số thống kê chi tiết</returns>
        Task<SensorStatistics> GetSensorStatisticsAsync(int sensorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy tất cả các sự kiện cảnh báo từ các cảm biến trong một phòng
        /// dựa trên ngưỡng giới hạn cho từng loại cảm biến
        /// </summary>
        /// <param name="roomId">ID của phòng</param>
        /// <param name="startDate">Thời gian bắt đầu</param>
        /// <param name="endDate">Thời gian kết thúc</param>
        /// <returns>Từ điển với khóa là ID cảm biến và giá trị là danh sách các sự kiện cảnh báo</returns>
        Task<Dictionary<int, List<AlertEvent>>> GetRoomAlertsAsync(int roomId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy dữ liệu đọc từ cảm biến dưới dạng các điểm dữ liệu biểu đồ
        /// </summary>
        /// <param name="sensorId">ID của cảm biến</param>
        /// <param name="startDate">Thời gian bắt đầu</param>
        /// <param name="endDate">Thời gian kết thúc</param>
        /// <returns>Danh sách các điểm dữ liệu cho biểu đồ</returns>
        Task<List<ChartDataPoint>> GetSensorReadingsDataPointAsync(int sensorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Tính giá trị trung bình theo từng giờ trong ngày cho dữ liệu cảm biến
        /// </summary>
        /// <param name="sensorId">ID của cảm biến</param>
        /// <param name="startDate">Thời gian bắt đầu</param>
        /// <param name="endDate">Thời gian kết thúc</param>
        /// <returns>Từ điển chứa danh sách các điểm dữ liệu trung bình theo giờ</returns>
        Task<Dictionary<string, List<ChartDataPoint>>> GetHourlyAveragesAsync(int sensorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Tính giá trị trung bình theo ngày cho dữ liệu cảm biến
        /// </summary>
        /// <param name="sensorId">ID của cảm biến</param>
        /// <param name="startDate">Thời gian bắt đầu</param>
        /// <param name="endDate">Thời gian kết thúc</param>
        /// <returns>Danh sách các điểm dữ liệu trung bình theo ngày</returns>
        Task<List<ChartDataPoint>> GetDailyAveragesAsync(int sensorId, DateTime startDate, DateTime endDate);
    }
}