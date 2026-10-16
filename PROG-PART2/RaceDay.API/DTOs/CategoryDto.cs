namespace RaceDay.API.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal EntryFee { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}