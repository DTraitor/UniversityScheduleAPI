using DataAccess.Models;
using DataAccess.Models.Interface;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Readers.Interfaces;

public interface IScheduleReader<T>
{
    Task<(IEnumerable<IModifiedEntry>, IEnumerable<T>)> ReadSchedule(CancellationToken cancellationToken);
    IEnumerable<T> ReadGroupsList(HtmlDocument document);
}