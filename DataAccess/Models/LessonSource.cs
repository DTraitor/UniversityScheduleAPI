using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class LessonSource : IEntityId
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string HrefId { get; set; }
    public LessonSourceType SourceType { get; set; }

    public string PageHash { get; set; }
}