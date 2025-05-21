using CleanroomMonitoring.Web.Models;

namespace CleanroomMonitoring.Web.ViewModels
{
    public class SensorReadingData
    {
        public SensorInfo SensorInfo { get; set; }
        public SensorReading LatestReading { get; set; }
        public List<SensorReading> Readings { get; set; }
        public SensorConfig SensorConfig { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal AvgValue { get; set; }
    }
}
