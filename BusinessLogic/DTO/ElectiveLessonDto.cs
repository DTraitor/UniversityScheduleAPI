namespace BusinessLogic.DTO;

public record ElectiveLessonDto
{
    public string Title { get; set; }
    public int SourceId { get; set; }
    public List<string> Types { get; set; }
}