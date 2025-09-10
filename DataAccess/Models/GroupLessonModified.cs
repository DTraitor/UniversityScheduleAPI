using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class GroupLessonModified : IModifiedEntry
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public ModifiedState State { get; set; }

    public int GetKey()
    {
        return GroupId;
    }
}