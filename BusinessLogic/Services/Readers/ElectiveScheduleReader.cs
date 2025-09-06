using DataAccess.Models;
using HtmlAgilityPack;
using BusinessLogic.Services.Readers.Interfaces;

namespace BusinessLogic.Services.Readers;

public class ElectiveScheduleReader : IElectiveScheduleReader
{
    public IEnumerable<ElectiveLesson> ReadLessons(HtmlDocument document)
    {
        throw new NotImplementedException();
    }
}