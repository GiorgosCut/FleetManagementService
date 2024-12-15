using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Container
    {
        [Required]
        public int Id { get; set; }
        public Vessel? Vessel { get; set; }
    }
}
