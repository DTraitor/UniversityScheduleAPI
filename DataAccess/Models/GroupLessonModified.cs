using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class GroupLessonModified : IModifiedEntry, IEntityId
{
    public int Id { get; set; }
    public int GroupId { get; set; }

    public int Key => GroupId;
}