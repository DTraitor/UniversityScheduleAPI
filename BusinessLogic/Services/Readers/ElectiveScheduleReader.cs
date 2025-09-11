using BusinessLogic.Services.Readers.Interfaces;
using DataAccess.Models;
using DataAccess.Models.Interface;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Readers;

public class ElectiveScheduleReader : IScheduleReader<ElectiveLesson, ElectiveLessonModified>
{
    public Task<(IEnumerable<ElectiveLessonModified>, IEnumerable<ElectiveLesson>)> ReadSchedule(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ElectiveLesson> ReadGroupsList(HtmlDocument document)
    {
        throw new NotImplementedException();
    }
}