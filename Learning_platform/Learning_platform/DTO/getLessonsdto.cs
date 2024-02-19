using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class getLessonsdto
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public IFormFile VideoFile { get; set; }
    }
}
