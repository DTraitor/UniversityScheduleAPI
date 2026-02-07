using System.Data.Common;
using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserModifiedRepository
{
    Task<IEnumerable<UserModified>> GetNotProcessed(CancellationToken cancellationToken = default);
    void Add(int userId, DbTransaction transaction);
    void RemoveProcessed(IEnumerable<UserModified> toRemove, DbTransaction transaction);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}