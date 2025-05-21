
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CleanroomMonitoring.Web.Models
{
    [Table("Factory")]
    public class Factory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int FactoryID { get; set; }
        public string? FactoryName { get; set; }
        public string? Location { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public int? CreatedByUserID { get; set; }
        public DateTime? LastModifiedDate { get; set; } = DateTime.Now;
        public int? LastModifiedByUserID { get; set; }
        public string? COMMENT { get; set; }
        public ICollection<CleanRoom> CleanRooms { get; set; }
    }
}
