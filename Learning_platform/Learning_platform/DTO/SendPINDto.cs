using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class SendPINDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
