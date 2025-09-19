namespace BusinessLogic.Services.Interfaces;

public interface IChangeHandler<T>
{
    Task HandleChanges(IEnumerable<T> oldLessons, IEnumerable<T> newLessons, CancellationToken token);
}