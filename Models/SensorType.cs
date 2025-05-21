using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.Models
{
    [Table("SensorType", Schema = "dbo")]
    public class SensorType
    {
        [Key]
        public int SensorTypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string TypeName { get; set; }

        [StringLength(20)]
        public string Unit { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public virtual ICollection<SensorInfo> SensorInfos { get; set; }
    }
}
