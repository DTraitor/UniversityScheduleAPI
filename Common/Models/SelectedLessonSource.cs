using Common.Enums;
using Common.Models.Interface;

namespace Common.Models;

public class SelectedLessonSource : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SourceId { get; set; }
    public int SubGroupNumber { get; set; }
    public LessonSourceType LessonSourceType { get; set; }

    //User alert related
    public string SourceName { get; set; }
}