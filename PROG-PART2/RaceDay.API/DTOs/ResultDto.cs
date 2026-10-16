namespace RaceDay.API.DTOs
{
    public class ResultDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
        public TimeSpan? Pace { get; set; }
        public bool IsDisqualified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}