using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.Models
{

    [Table("MaintenanceEvent")]
    public class MaintenanceEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Event ID")]
        public int EventID { get; set; }
        [Display(Name = "Room ID")]
        public int? RoomID { get; set; }
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }
        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }
        [MaxLength(50)]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }
        [MaxLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "Created By User ID")]
        public int? CreatedByUserID { get; set; }
        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }
        [MaxLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; }
        [Display(Name = "Approved By User ID")]
        public int? ApprovedByUserID { get; set; }
        [Display(Name = "Approval Date")]
        public DateTime? ApprovalDate { get; set; }
        [MaxLength(500)]
        [Display(Name = "COMMENT")]
        public string COMMENT { get; set; }
    }

}
