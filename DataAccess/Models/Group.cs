using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class Group : IEntityId
{
    public int Id { get; set; }
    public string GroupName { get; set; }
    public string FacultyName { get; set; }
    public string SchedulePageHash { get; set; }
}