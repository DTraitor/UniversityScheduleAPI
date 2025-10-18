using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class ElectiveLessonDay : IEntityId
{
    public int Id { get; set; }
    public int DayId { get; set; }
    public int HourId { get; set; }
}