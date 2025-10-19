namespace BusinessLogic.DTO;

public record ElectiveSelectedLessonDto
{
    public IEnumerable<SourceDto> Sources { get; init; }
    public IEnumerable<EntryDto> Entries { get; init; }

    public record SourceDto
    {
        public string Name { get; init; }
        public string? Type { get; init; }
        public int SelectedSourceId { get; init; }
        public int SubGroupNumber { get; init; }
    }

    public record EntryDto
    {
        public int SelectedEntryId { get; init; }
        public string EntryName { get; init; }
        public string? Type { get; init; }
        public bool WeekNumber { get; init; }
        public DayOfWeek DayOfWeek { get; init; }
        public TimeSpan StartTime { get; init; }
    }
}
