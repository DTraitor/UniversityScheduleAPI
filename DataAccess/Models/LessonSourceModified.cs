using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class LessonSourceModified : IEntityId
{
    public int Id { get; set; }
    public int SourceId { get; set; }
}