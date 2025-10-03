namespace BusinessLogic.Services.Interfaces;

public interface IChangeHandler<T>
{
    Task HandleChanges(IEnumerable<T> oldLessons, ICollection<T> newLessons, CancellationToken token);
}