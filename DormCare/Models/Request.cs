namespace DormCare.Models
{
    public class Request
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
        public string Text { get; set; }
        public string Type { get; set; }
        public string Status { get; set; } = "New";
    }
}
