using CleanroomMonitoring.Web.Models;

using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.ViewModels
{
    public class SensorViewModel3
    {
        public int? SensorInfoID { get; set; }
        public SensorInfo SensorInfo { get; set; }
        public IEnumerable<SensorInfo> SensorList { get; set; }
        public IEnumerable<SensorReading> SensorReadings { get; set; }
        public TimeRange SelectedTimeRange { get; set; } = TimeRange.FourHours;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public enum TimeRange
    {
        [Display(Name = "4 giờ")]
        FourHours,
        [Display(Name = "8 giờ")]
        EightHours,
        [Display(Name = "24 giờ")]
        TwentyFourHours,
        [Display(Name = "Tùy chỉnh")]
        Custom
    }
}