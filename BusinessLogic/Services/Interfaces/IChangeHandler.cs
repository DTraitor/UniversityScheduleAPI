namespace BusinessLogic.Services.Interfaces;

public interface IChangeHandler<T>
{
    Task<IEnumerable<T>> HandleChanges(IEnumerable<T> oldLessons, ICollection<T> newLessons, CancellationToken token);
}