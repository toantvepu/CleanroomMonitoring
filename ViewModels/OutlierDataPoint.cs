 

namespace CleanroomMonitoring.Web.ViewModels
{
    public class OutlierDataPoint : ChartDataPoint
    {
        public decimal DeviationFromMean { get; set; }
        public string Severity { get; set; }  // "Minor", "Moderate", "Severe"
    }
}
