using DataAccess.Models;
using HtmlAgilityPack;

namespace Reader.Services.Interfaces;

public interface IGroupScheduleReader
{
    IEnumerable<ScheduleLesson> ReadLessons(HtmlDocument document, int groupId, CancellationToken stoppingToken);
}