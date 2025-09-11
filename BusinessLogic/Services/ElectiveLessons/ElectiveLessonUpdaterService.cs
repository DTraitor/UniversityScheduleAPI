using BusinessLogic.Services.Interfaces;
using DataAccess.Models;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveLessonUpdaterService : ILessonUpdaterService<ElectiveLesson, ElectiveLessonModified>
{
    public Task ProcessModifiedEntry(ElectiveLessonModified modifiedEntry)
    {
        throw new NotImplementedException();
    }
}