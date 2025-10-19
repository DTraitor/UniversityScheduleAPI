namespace BusinessLogic.DTO;

public record ElectiveSubgroupsDto
{
    public int LessonSourceId { get; init; }
    public IEnumerable<int> PossibleSubgroups { get; init; }
};