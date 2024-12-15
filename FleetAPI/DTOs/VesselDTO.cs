using FleetAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace FleetAPI.DTOs
{
    public class VesselDTO
    {        
        [Required]
        public int Capacity { get; set; }

        public string? Fleet { get; set; }
    }
}
