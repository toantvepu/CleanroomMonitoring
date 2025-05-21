using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.Models
{
    [Table("AuditTrail", Schema = "dbo")]
    public class AuditTrail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditID { get; set; }

        public DateTime? EventTime { get; set; }

        public int? UserID { get; set; }

        [StringLength(50)]
        public string EventType { get; set; }

        [StringLength(100)]
        public string TableName { get; set; }

        [StringLength(100)]
        public string RecordID { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        [StringLength(50)]
        public string IPAddress { get; set; }

        [StringLength(100)]
        public string ApplicationName { get; set; }

        [StringLength(500)]
        public string COMMENT { get; set; }
    }
}
