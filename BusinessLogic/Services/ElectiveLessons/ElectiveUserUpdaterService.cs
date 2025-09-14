using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveUserUpdaterService : IUserLessonUpdaterService<ElectiveLesson>
{
    public Task ProcessModifiedUser(UserModified modifiedUser)
    {
        throw new NotImplementedException();
    }

    public ProcessedByEnum ProcessedBy => ProcessedByEnum.ElectiveLessons;
}