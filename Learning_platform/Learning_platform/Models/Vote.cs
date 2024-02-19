namespace Learning_platform.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public Course Course { get; set; }
    }
}
