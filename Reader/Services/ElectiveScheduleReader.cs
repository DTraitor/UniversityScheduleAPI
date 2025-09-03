using DataAccess.Models;
using HtmlAgilityPack;
using Reader.Services.Interfaces;

namespace Reader.Services;

public class ElectiveScheduleReader : IElectiveScheduleReader
{
    public IEnumerable<ElectiveLesson> ReadLessons(HtmlDocument document)
    {
        throw new NotImplementedException();
    }
}