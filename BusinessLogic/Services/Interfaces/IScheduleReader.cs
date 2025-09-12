using DataAccess.Models.Interface;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Interfaces;

public interface IScheduleReader<T, TModifiedEntry> where TModifiedEntry : IModifiedEntry
{
    Task<(IEnumerable<TModifiedEntry>, IEnumerable<T>)> ReadSchedule(CancellationToken cancellationToken);
}