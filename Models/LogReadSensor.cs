using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanroomMonitoring.Web.Models
{
    [Table("LogReadSensor", Schema = "dbo")]
    public class LogReadSensor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LogReadSensorID { get; set; }

        public string Message { get; set; }
        public string MessageType { get; set; }
        public DateTime LogTime { get; set; }
    }
}
