using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanroomMonitoring.Web.Models
{
    [Table("SensorHealthCheckHistory", Schema = "dbo")]
    public class SensorHealthCheckHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HealthCheckID { get; set; }

        public int SensorInfoID { get; set; }

        public DateTime CheckTime { get; set; }
        public string? Status { get; set; } // OK, WARNING, ERROR, RECOVERED
        public string? IssueType { get; set; }
        public string? Description { get; set; }

        // Khóa ngoại
        public virtual SensorInfo? SensorInfo { get; set; }
    }
}
