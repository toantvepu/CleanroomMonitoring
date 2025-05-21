namespace CleanroomMonitoring.Web.ViewModels
{
    public class SensorStatusViewModel
    {
        public int SensorInfoID { get; set; }
        public decimal? CurrentValue { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? AverageValue { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; } // "Normal", "Warning", "Alert"
    }
}
