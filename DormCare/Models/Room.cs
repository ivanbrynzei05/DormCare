using System.Collections.Generic;

namespace DormCare.Models
{
    public class Room
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string Number { get; set; }

        [System.ComponentModel.DataAnnotations.Range(1, 100)]
        public int Capacity { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
