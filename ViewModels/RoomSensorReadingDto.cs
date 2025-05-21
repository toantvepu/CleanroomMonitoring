namespace CleanroomMonitoring.Web.ViewModels
{
    public class RoomSensorReadingDto
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
        public string Type { get; set; }
    }
}
