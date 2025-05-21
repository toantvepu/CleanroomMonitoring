using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanroomMonitoring.Web.Models
{
    [Table("SensorFlags", Schema = "dbo")]
    public class SensorFlags
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FlagID { get; set; }
        public int SensorInfoID { get; set; }
        // Cờ giá trị bất thường
        public bool HasAbnormalValue { get; set; }
        public DateTime? AbnormalValueTime { get; set; }
        public string? AbnormalValueType { get; set; } // ABOVE_THRESHOLD, BELOW_THRESHOLD, etc.
        public string? AbnormalValueDescription { get; set; }
        public DateTime? NormalizedTime { get; set; }

        // Cờ hoạt động chập chờn
        public bool IsFlickering { get; set; }
        public DateTime? LastHealthCheckTime { get; set; }
        public int RecordsInLastHour { get; set; }
        public int RecordsInLastDay { get; set; }
        public int DisconnectsInLastDay { get; set; }

        // Khóa ngoại
        public virtual SensorInfo? SensorInfo { get; set; }

    }
}
