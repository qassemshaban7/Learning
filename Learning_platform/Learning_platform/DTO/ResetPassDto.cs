using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class ResetPassDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public int pin { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Pass { get; set; }
    }
}
