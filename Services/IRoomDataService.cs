using CleanroomMonitoring.Web.ViewModels;

namespace CleanroomMonitoring.Web.Services
{
    public interface IRoomDataService
    {
        Task<IEnumerable<CleanRoomDashboardViewModel>> GetDashboardDataAsync(int hours = 24);
         
        Task<RoomDetailDto> GetRoomDetailAsync(int roomId);
        Task<IEnumerable<RoomSensorReadingDto>> GetRoomTemperatureDataAsync(int roomID, int hour = 24); 
        Task<IEnumerable<RoomSensorReadingDto>> GetRoomHumidityDataAsync(int roomID, int hour = 24);
        Task<IEnumerable<RoomSensorReadingDto>> GetRoomPressureDataAsync(int roomID, int hour = 24);

    }
}
