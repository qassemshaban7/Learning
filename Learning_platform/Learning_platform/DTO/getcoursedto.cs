using System.ComponentModel.DataAnnotations;

namespace Learning_platform.DTO
{
    public class getcoursedto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string ImageOfCertificate { get; set; }
        public int Price { get; set; }
        public int vote { get; set; }   

        public bool Favorate { get; set; }

    }
}
