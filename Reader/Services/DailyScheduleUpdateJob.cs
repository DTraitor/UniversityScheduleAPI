using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reader.Services.Interfaces;

namespace Reader.Services;

public class DailyScheduleUpdateService : BackgroundService
{
    private readonly IElectiveScheduleReader _electiveScheduleReader;
    private readonly IGroupScheduleReader _groupScheduleReader;
    private readonly ILogger<DailyScheduleUpdateService> _logger;

    public DailyScheduleUpdateService(
        IElectiveScheduleReader electiveScheduleReader,
        IGroupScheduleReader groupScheduleReader,
        ILogger<DailyScheduleUpdateService> logger)
    {
        _electiveScheduleReader = electiveScheduleReader;
        _groupScheduleReader = groupScheduleReader;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyScheduleUpdateService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int hourSpan = 24 - DateTime.Now.Hour;
            int numberOfHours = hourSpan;

            if (hourSpan == 24)
            {
                _logger.LogInformation("Running daily tasks at {Time}", DateTime.Now);

                //TODO: Update all schedules

                numberOfHours = 24;
            }

            _logger.LogInformation("Next run scheduled after {NumberOfHours} hours.", numberOfHours);

            try
            {
                await Task.Delay(TimeSpan.FromHours(numberOfHours), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // service stopping
            }
        }
    }
}