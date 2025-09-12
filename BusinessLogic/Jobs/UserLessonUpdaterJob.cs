using Microsoft.Extensions.Hosting;

namespace BusinessLogic.Jobs;

public class UserLessonUpdaterJob<T> : IHostedService, IDisposable
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}