using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class CategoryDTO
    { 
        [Required]
        public string Name { get; set; }
    }
}
