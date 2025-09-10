using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Readers.Interfaces;

public interface IScheduleParser<T>
{
    bool HasHashChanged(HtmlDocument document, string oldHash, out string newHash);
    IEnumerable<T> ReadLessons(HtmlDocument document);
}