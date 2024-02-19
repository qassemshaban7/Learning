using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Learning_platform.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? Image { get; set; }

        public int? PasswordResetPin { get; set; } = null;

        public DateTime? ResetExpires { get; set; } = null;
    }
}
