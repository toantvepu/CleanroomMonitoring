
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.Models
{
    /// <summary>
    /// Lưu lịch sử cảnh báo khi có dữ liệu vượt ngưỡng
    /// </summary>
    [Table("AlertHistory", Schema = "dbo")]
    public class AlertHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AlertID { get; set; }

        public int SensorInfoID { get; set; }
        public DateTime? AlertTime { get; set; }


        public string? AlertType { get; set; }
        public string? AlertMessage { get; set; }
         
        public decimal? AlertValue { get; set; }
        public bool IsHandled { get; set; }
        public SensorInfo? SensorInfo { get; set; }
    }
}
