 
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;

namespace CleanroomMonitoring.Web.Models
{
    [Table("SensorInfo", Schema = "dbo")]
    public class SensorInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SensorInfoID { get; set; }

        public int? RoomID { get; set; }

        public int? SensorTypeID { get; set; }

        [StringLength(200)]
        public string SensorName { get; set; }

        public int? ModbusAddress { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(50)]
        public string Phase { get; set; }

        public bool  IsActive { get; set; }

        [StringLength(500)]
        public string? COMMENT { get; set; }

        [ForeignKey("RoomID")]
        public virtual CleanRoom? CleanRoom { get; set; }

        [ForeignKey("SensorTypeID")]
        public virtual SensorType? SensorType { get; set; }

        /// <summary>
        /// Cho phép null
        /// </summary>
        public virtual ICollection<SensorReading>? SensorReadings { get; set; }
     
        [ValidateNever] //sử dụng ? như trên là như nhau
        public virtual ICollection<SensorLocation>? SensorLocations { get; set; }

        public virtual ICollection<SensorConnectionStatus>? SensorConnectionStatuses { get; set; }
        public virtual ICollection<SensorFlags>? SensorFlags { get; set; }
         

        /// <summary>
        /// ASP.NET Core mặc định coi các property reference type (class, collection) là required
        /// ASP.NET Core mặc định coi các property reference type (class, collection) là required nếu không nullable.
        /// Để tránh bị validate required, chỉ cần:
        /// Thêm dấu ? (nullable reference type)
        /// Hoặc dùng[ValidateNever] (bỏ qua validation cho property đó)
        /// </summary>
    }

}
