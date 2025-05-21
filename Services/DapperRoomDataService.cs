using CleanroomMonitoring.Web.Data;
using CleanroomMonitoring.Web.ViewModels;

using Microsoft.Extensions.Caching.Memory;

using System.Data;

namespace CleanroomMonitoring.Web.Services
{
    public class DapperRoomDataService: IRoomDataService
    {
        private readonly IDbConnection _dbConnection;
        private readonly DapperHelper _dapperHelper;
        private readonly IMemoryCache _cache;
        public DapperRoomDataService(DapperHelper dapperHelper,IMemoryCache cache)
        {
            _dapperHelper = dapperHelper;
            _cache = cache;
        }

        public async Task<IEnumerable<CleanRoomDashboardViewModel>> GetDashboardDataAsync(int hours = 24)
        {
            string cacheKey = $"DashboardData_{hours}";

            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out IEnumerable<CleanRoomDashboardViewModel> cachedData)) {
                return cachedData;
            }

            // Gọi stored procedure để lấy dữ liệu
            var data = await _dapperHelper.ExecuteStoredProcedureAsync<CleanRoomDashboardViewModel>(
                "GetCleanRoomDashboardData_CaiTienHieuNang",
                new { Hours = hours }
                );
            //); var data = await _dapperHelper.ExecuteStoredProcedureAsync<CleanRoomDashboardViewModel>(
            //    "GetCleanRoomDashboardData",
            //    new { Hours = hours }
            //);

            // Lưu vào cache trong 5 phút
            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));

            return data;
        }
        public async Task<RoomDetailDto> GetRoomDetailAsync(int roomId)
        {
            var result = await _dapperHelper.ExecuteStoredProcedureAsync<RoomDetailDto>(
                "GetRoomDetailData",
                new { RoomID = roomId }
            );

            return result.FirstOrDefault();
        }
        public async Task<IEnumerable<RoomSensorReadingDto>> GetRoomTemperatureDataAsync(int roomId, int hours = 24)
        {
            string cacheKey = $"RoomTemp_{roomId}_{hours}";

            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out IEnumerable<RoomSensorReadingDto> cachedData)) {
                return cachedData;
            }

            // Gọi stored procedure
            var result = await _dapperHelper.ExecuteStoredProcedureAsync<RoomSensorReadingDto>(
                "GetRoomDetailData",
                new { RoomID = roomId, Hours = hours }
            );

            var data = result.Where(r => r.Type == "Temperature").ToList();

            // Lưu vào cache
            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));

            return data;
        }
        public async Task<IEnumerable<RoomSensorReadingDto>> GetRoomHumidityDataAsync(int roomId, int hours = 24)
        {
            string cacheKey = $"RoomHumidity_{roomId}_{hours}";

            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out IEnumerable<RoomSensorReadingDto> cachedData)) {
                return cachedData;
            }

            // Gọi stored procedure
            var result = await _dapperHelper.ExecuteStoredProcedureAsync<RoomSensorReadingDto>(
                "GetRoomDetailData",
                new { RoomID = roomId, Hours = hours }
            );

            var data = result.Where(r => r.Type == "Humidity").ToList();

            // Lưu vào cache
            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));

            return data;
        }

        public async Task<IEnumerable<RoomSensorReadingDto>> GetRoomPressureDataAsync(int roomId, int hours = 24)
        {
            string cacheKey = $"RoomPressure_{roomId}_{hours}";

            // Kiểm tra cache
            if (_cache.TryGetValue(cacheKey, out IEnumerable<RoomSensorReadingDto> cachedData)) {
                return cachedData;
            }

            // Gọi stored procedure
            var result = await _dapperHelper.ExecuteStoredProcedureAsync<RoomSensorReadingDto>(
                "GetRoomDetailData",
                new { RoomID = roomId, Hours = hours }
            );

            var data = result.Where(r => r.Type == "Pressure").ToList();

            // Lưu vào cache
            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));

            return data;
        }
    }
     
}
