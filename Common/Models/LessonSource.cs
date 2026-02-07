using Common.Enums;
using Common.Models.Interface;

namespace Common.Models;

public class LessonSource : IEntityId
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string HrefId { get; set; }
    public LessonSourceType SourceType { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TimeZone { get; set; }

    public string PageHash { get; set; }
}