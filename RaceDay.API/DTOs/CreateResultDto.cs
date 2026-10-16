using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateResultDto
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        public int? Position { get; set; }

        public TimeSpan? Pace { get; set; }

        public bool IsDisqualified { get; set; } = false;
    }
}