using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Learning_platform.DTO
{
    public class InstructorDTO
    {
        internal string Photo;
        internal  string Photo12;

        public string Name { get; set; }
        public IFormFile Imageprofile { get; set; }
        public string Description { get; set; }
        //[JsonIgnore]
        //public string Photo { get; set; }
    }
}
