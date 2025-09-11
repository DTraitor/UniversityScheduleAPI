using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class ElectiveLessonModified : IModifiedEntry
{
    public int Id { get; set; }
    public int ElectiveDayId { get; set; }

    public int Key => ElectiveDayId;
}