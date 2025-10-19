using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class SelectedLessonSource : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SourceId { get; set; }
    public int SubGroupNumber { get; set; }
    public string? Type { get; set; }
    public LessonSourceType LessonSourceType { get; set; }

    //User alert related
    public string SourceName { get; set; }
}