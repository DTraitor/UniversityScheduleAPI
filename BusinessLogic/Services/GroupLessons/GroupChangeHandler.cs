using BusinessLogic.Services.Interfaces;
using DataAccess.Models;

namespace BusinessLogic.Services.GroupLessons;

public class GroupChangeHandler : IChangeHandler<GroupLesson>
{
    public async Task<IEnumerable<GroupLesson>> HandleChanges(IEnumerable<GroupLesson> oldLessons, ICollection<GroupLesson> newLessons, CancellationToken token)
    {
        return [];
    }
}