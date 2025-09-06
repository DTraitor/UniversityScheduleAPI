using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Readers.Interfaces;

public interface IElectiveScheduleReader
{
    IEnumerable<ElectiveLesson> ReadLessons(HtmlDocument document);
}