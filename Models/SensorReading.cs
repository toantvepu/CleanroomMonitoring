using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanroomMonitoring.Web.Models
{
    [Table("SensorReading", Schema = "dbo")]
    public class SensorReading
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReadingID { get; set; }

        [Required]
        public int SensorInfoID { get; set; }

        [Column(TypeName = "decimal(7, 1)")]
        public decimal? ReadingValue { get; set; }

        public DateTime? ReadingTime { get; set; }

        public bool? IsValid { get; set; }

        [ForeignKey("SensorInfoID")]
        public virtual SensorInfo SensorInfo { get; set; }
    }
}
