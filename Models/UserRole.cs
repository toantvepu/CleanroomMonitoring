using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.Models
{
    [Table("UserRole", Schema = "dbo")]
    public class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [StringLength(500)]
        public string COMMENT { get; set; }

        public virtual ICollection<UserRoleMapping> UserRoleMappings { get; set; }
    }

}
