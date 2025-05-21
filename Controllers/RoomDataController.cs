using CleanroomMonitoring.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace CleanroomMonitoring.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomDataController : ControllerBase
    {
        private readonly IRoomDataService _roomDataService;

        public RoomDataController(IRoomDataService roomDataService)
        {
            _roomDataService = roomDataService;
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoomData(int roomId, int hours = 24)
        {
            var temperatureData = await _roomDataService.GetRoomTemperatureDataAsync(roomId, hours);
            var humidityData = await _roomDataService.GetRoomHumidityDataAsync(roomId, hours);
            var pressureData = await _roomDataService.GetRoomPressureDataAsync(roomId, hours);

            return Ok(new {
                Temperature = temperatureData,
                Humidity = humidityData,
                Pressure = pressureData
            });
        }
    }
}
