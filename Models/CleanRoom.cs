 
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.Models
{
    [Table("CleanRoom", Schema = "dbo")]
    public class CleanRoom
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomID { get; set; }

        public int? FactoryID { get; set; }

        [Required]
        [StringLength(50)]
        public string RoomName { get; set; }

        [StringLength(50)]
        public string Area { get; set; } 
        public string? Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? CreatedByUserID { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public int? LastModifiedByUserID { get; set; }

        [StringLength(20)]
        public string CleanRoomClass { get; set; }

        [StringLength(500)]
        public string COMMENT { get; set; }

        [ForeignKey("FactoryID")]
        public virtual Factory? Factory { get; set; }

        public virtual ICollection<SensorInfo>? SensorInfos { get; set; }
    }

}
