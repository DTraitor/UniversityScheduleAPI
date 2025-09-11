using HtmlAgilityPack;

namespace BusinessLogic.Services.Interfaces;

public interface IScheduleParser<T>
{
    bool HasHashChanged(HtmlDocument document, string oldHash, out string newHash);
    IEnumerable<T> ReadLessons(HtmlDocument document);
}