using System.ComponentModel.DataAnnotations;

namespace Learning_platform.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        public string Image { get; set; }
        public string Description { get; set; }
        public ICollection<Course> Courses { get; set; }
    }
}
