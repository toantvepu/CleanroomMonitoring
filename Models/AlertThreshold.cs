using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.Models
{
    [Table("AlertThreshold", Schema = "dbo")]
    public class AlertThreshold
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ThresholdID { get; set; }
        public int SensorInfoID { get; set; }



        public decimal? MinValue { get; set; }

        public decimal? MaxValue { get; set; }

        public decimal? WarningMinValue { get; set; }

        public decimal? WarningMaxValue { get; set; }

        public int? AlertDelay { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? UpdatedByUserID { get; set; }

        [StringLength(20)]
        public string ApprovalStatus { get; set; }

        public int? ApprovedByUserID { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [StringLength(500)]
        public string COMMENT { get; set; }
    }
}
