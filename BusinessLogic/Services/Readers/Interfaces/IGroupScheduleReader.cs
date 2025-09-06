using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Readers.Interfaces;

public interface IGroupScheduleReader
{
    IEnumerable<ScheduleLesson>? ReadLessons(HtmlDocument document, Group group, CancellationToken stoppingToken);
}