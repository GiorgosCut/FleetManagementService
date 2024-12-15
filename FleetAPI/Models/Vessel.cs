using FleetAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Vessel
    {
        [Required]
        public int Id {  get; set; }
        [Required]
        public int Capacity { get; set; }
        public string? Fleet {  get; set; }
        public List<Container> Containers = new();
    }
}
