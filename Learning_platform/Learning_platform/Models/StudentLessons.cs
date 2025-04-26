using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_platform.Models
{
    public class StudentLessons
    {
        public int Id { get; set; }


        [ForeignKey("LessonId")]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        [ForeignKey("StudentId")]
        public string StudentId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
