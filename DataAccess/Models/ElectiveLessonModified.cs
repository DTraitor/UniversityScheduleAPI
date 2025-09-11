using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class ElectiveLessonModified : IModifiedEntry
{
    public int Id { get; set; }
    public int ElectiveDayId { get; set; }
    public ModifiedState State { get; set; }

    public int GetKey()
    {
        return ElectiveDayId;
    }
}