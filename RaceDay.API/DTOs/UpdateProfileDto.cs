using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        // Organiser-specific
        [StringLength(100)]
        public string? OrganisationName { get; set; }

        [StringLength(255)]
        public string? Website { get; set; }

        // Participant-specific
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(500)]
        public string? MedicalConditions { get; set; }
    }
}