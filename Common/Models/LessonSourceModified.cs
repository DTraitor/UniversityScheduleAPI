using Common.Models.Interface;

namespace Common.Models;

public class LessonSourceModified : IEntityId
{
    public int Id { get; set; }
    public int SourceId { get; set; }
}