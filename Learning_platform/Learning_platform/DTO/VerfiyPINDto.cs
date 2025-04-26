using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class VerfiyPINDto
    {
        [Required]
        public int pin { get; set; }
    }
}
