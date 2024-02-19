using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class AddCourseDTO
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public IFormFile ImageOfCertificate { get; set; }
        public int Price { get; set; }
        [Required]
        public int Category_Id { get; set; }
        [Required]
        public int Instructor_Id { get; set; }
    }
}
