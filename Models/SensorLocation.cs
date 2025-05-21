using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.Models
{
    [Table("SensorLocation")]
    public class SensorLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LocationID { get; set; }

        public int? SensorInfoID { get; set; }

        [StringLength(100)]
        public string? LocationName { get; set; }

        [StringLength(10)]
        public string? XCoordinate { get; set; } //Vị trí trên trục X

        [StringLength(10)]
        public string? YCoordinate { get; set; }  //Vị trí trên trục Y

        [StringLength(10)]
        public string? ZCoordinate { get; set; }  //Vị trí trên trục Z  

        public bool  IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? CreatedByUserID { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public int? LastModifiedByUserID { get; set; }

        [StringLength(500)]
        public string? COMMENT { get; set; }
        public int? ToaDoX { get; set; }
        public int? ToaDoY { get; set; }
        public string? OverlayDirection { get; set; }
        // Navigation property
        [ForeignKey("SensorInfoID")]
        public virtual SensorInfo? SensorInfo { get; set; }
    }
}
