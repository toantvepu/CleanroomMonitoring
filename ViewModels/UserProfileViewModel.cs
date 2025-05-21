using System.ComponentModel.DataAnnotations;

namespace CleanroomMonitoring.Web.ViewModels
{
    public class UserProfileViewModel
    {
        public int UserID { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }
    }
}