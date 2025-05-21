namespace CleanroomMonitoring.Web.ViewModels
{
    public class CleanRoomDashboardViewModel
    {
        public int RoomID { get; set; }
        public string? RoomName { get; set; }
        public string? Area { get; set; }
        public string? CleanRoomClass { get; set; }

        //Nhiệt độ
        public decimal? CurrentTemperature { get; set; }
        public decimal? MinTemperature { get; set; }
        public decimal? MaxTemperature { get; set; }
        public decimal? AvgTemperature { get; set; }
        public int InvalidTemperatureCount { get; set; }

        //Độ ẩm
        public decimal? CurrentHumidity { get; set; }
        public decimal? MinHumidity { get; set; }
        public decimal? MaxHumidity { get; set; }
        public decimal? AvgHumidity { get; set; }
        public int InvalidHumidityCount { get; set; }

        //Áp suất
        public decimal? CurrentPressure { get; set; }
        public decimal? MinPressure { get; set; }
        public decimal? MaxPressure { get; set; }
        public decimal? AvgPressure { get; set; }
        public int InvalidPressureCount { get; set; }

    }
   
}
