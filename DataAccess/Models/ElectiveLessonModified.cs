using DataAccess.Enums;

namespace DataAccess.Models;

public class ElectiveLessonModified
{
    public int Id { get; set; }
    public int ElectiveDayId { get; set; }
    public ModifiedState State { get; set; }

    public int GetKey()
    {
        return ElectiveDayId;
    }
}