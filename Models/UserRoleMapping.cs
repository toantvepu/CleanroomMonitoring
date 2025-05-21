using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanroomMonitoring.Web.Models
{
    [Table("UserRoleMapping", Schema = "dbo")]
    public class UserRoleMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int UserID { get; set; }

        public int RoleID { get; set; }

        public DateTime? AssignedDate { get; set; }

        public int? AssignedBy { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("RoleID")]
        public virtual UserRole UserRole { get; set; }
    }
}
