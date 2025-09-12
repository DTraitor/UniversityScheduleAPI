using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserModifiedRepository
{
    Task<IEnumerable<UserModified>> GetNotProcessed(ProcessedByEnum flag, CancellationToken cancellationToken = default);
    Task ActivateBitFlag(int id, ProcessedByEnum flag);
    Task DeactivateBitFlag(int id, ProcessedByEnum flag);
}