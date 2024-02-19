using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class CheckCodeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public int pin { get; set; }
    }
}
