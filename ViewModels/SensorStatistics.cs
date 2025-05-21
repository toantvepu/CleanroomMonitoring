namespace CleanroomMonitoring.Web.ViewModels
{
    public class SensorStatistics
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal AvgValue { get; set; }
        public decimal StdDeviation { get; set; }
        public int TotalReadings { get; set; }
        public int ValidReadings { get; set; }
        public int InvalidReadings { get; set; }
        public int AlertCount { get; set; }
    }
}
