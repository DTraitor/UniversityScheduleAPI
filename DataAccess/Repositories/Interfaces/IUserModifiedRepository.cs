using Common.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess.Repositories.Interfaces;

public interface IUserModifiedRepository
{
    Task<ICollection<UserModified>> GetNotProcessedAsync(CancellationToken cancellationToken = default);
    void Add(int userId);
    Task RemoveProcessedAsync(ICollection<UserModified> toRemove);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}