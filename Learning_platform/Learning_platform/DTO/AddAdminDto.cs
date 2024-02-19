using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class AddAdminDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
    }
}
