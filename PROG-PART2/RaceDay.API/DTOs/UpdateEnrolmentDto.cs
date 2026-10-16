using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UpdateEnrolmentDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // "Registered", "Confirmed", "Cancelled"
    }
}