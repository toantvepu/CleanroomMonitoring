using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanroomMonitoring.Web.Models
{
    [Table("SensorConnectionStatus", Schema = "dbo")]
    public class SensorConnectionStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SensorConnectionStatusId { get; set; }
        public int SensorInfoID { get; set; }
        public bool IsConnected { get; set; }
        public DateTime? LastConnectionTime { get; set; }
        public DateTime? LastDisconnectionTime { get; set; }
        public int DisconnectionCount { get; set; }
        public string? LastIssueType { get; set; }
        public string? LastIssueDescription { get; set; }

        // Khóa ngoại
        [ForeignKey("SensorInfoID")]
        public virtual SensorInfo? SensorInfo { get; set; }

    }
}
