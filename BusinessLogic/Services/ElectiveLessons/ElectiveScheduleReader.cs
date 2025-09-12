using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveScheduleReader : IScheduleReader<ElectiveLesson, ElectiveLessonModified>
{
    public Task<(IEnumerable<ElectiveLessonModified>, IEnumerable<ElectiveLesson>)> ReadSchedule(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}