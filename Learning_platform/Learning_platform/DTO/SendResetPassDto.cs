using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class SendResetPassDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
