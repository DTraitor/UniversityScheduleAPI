using DataAccess.Models;
using HtmlAgilityPack;

namespace Reader.Services.Interfaces;

public interface IElectiveScheduleReader
{
    IEnumerable<ElectiveLesson> ReadLessons(HtmlDocument document);
}