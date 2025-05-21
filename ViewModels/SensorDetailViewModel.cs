 
namespace CleanroomMonitoring.Web.ViewModels
{
    public class SensorDetailViewModel
    {
        public int SensorInfoID { get; set; }
        public int RoomID { get; set; }
        public string SensorName { get; set; }
        public string SensorTypeName { get; set; }
        public string RoomName { get; set; }
        public string Unit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ChartDataPoint> ChartData { get; set; }

        // Add missing properties to fix CS1061  
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal AvgValue { get; set; }
        public int ValidReadings { get; set; }
        public int InvalidReadings { get; set; }
    }
}