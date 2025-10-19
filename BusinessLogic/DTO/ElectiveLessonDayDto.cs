namespace BusinessLogic.DTO;

public record ElectiveLessonDayDto
{
    public int SourceId { get; init; }
    public IEnumerable<ElectiveLessonSpecificDto> LessonDays { get; init; }

    public record ElectiveLessonSpecificDto
    {
        public int Id { get; init; }
        public string? Type { get; init; }
        public bool WeekNumber { get; init; }
        public DayOfWeek DayOfWeek { get; init; }
        public TimeSpan StartTime { get; init; }
    }
}