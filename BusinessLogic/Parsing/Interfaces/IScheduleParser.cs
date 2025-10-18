using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Parsing.Interfaces;

public interface IScheduleParser
{
    bool HasHashChanged(HtmlDocument document, string oldHash, out string newHash);
    ICollection<LessonEntry> ParseSchedule(HtmlDocument document);
}