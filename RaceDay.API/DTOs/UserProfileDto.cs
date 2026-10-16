namespace RaceDay.API.DTOs
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        // Organiser-specific
        public string? OrganisationName { get; set; }
        public string? Website { get; set; }

        // Participant-specific
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalConditions { get; set; }
    }
}