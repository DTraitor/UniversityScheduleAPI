using DataAccess.Models;
using DataAccess.Models.Interface;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Readers.Interfaces;

public interface IScheduleReader<T, TModifiedEntry> where TModifiedEntry : IModifiedEntry
{
    Task<(IEnumerable<TModifiedEntry>, IEnumerable<T>)> ReadSchedule(CancellationToken cancellationToken);
    IEnumerable<T> ReadGroupsList(HtmlDocument document);
}