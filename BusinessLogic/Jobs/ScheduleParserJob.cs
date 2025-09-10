using System.Collections.Concurrent;
using BusinessLogic.Mappers;
using BusinessLogic.Services.Readers.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class ScheduleParserJob<T> : IHostedService, IDisposable
{
    private const string SCHEDULE_API = "https://portal.nau.edu.ua";

    private readonly DateTimeOffset BEGIN_UNIVERSITY_DATE = DateTimeOffset.Parse("01-09-2025");
    private readonly DateTimeOffset END_UNIVERSITY_DATE = DateTimeOffset.Parse("31-12-2025");

    private readonly IScheduleReader<T> _scheduleReader;
    private readonly IRepository<T> _repository;
    private readonly IModifiedRepository<T> _modifiedRepository;

    // etc
    private IServiceProvider _services;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ScheduleParserJob<T>> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public ScheduleParserJob(
        IScheduleReader<T> scheduleReader,
        IRepository<T> repository,
        IServiceProvider services,
        IHttpClientFactory httpClientFactory,
        ILogger<ScheduleParserJob<T>> logger)
    {
        _scheduleReader = scheduleReader;
        _repository = repository;
        _services = services;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ScheduleParserJob123 starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ScheduleParserJob123 stopping...");

        _cancellationTokenSource.Cancel();
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ExecuteTimer(object? state)
    {
        lock (_executingLock)
        {
            ParseSchedule().Wait();
        }
    }

    private async Task ParseSchedule()
    {
        using var scope = _services.CreateScope();

        var persistentDataRepository = scope.ServiceProvider.GetRequiredService<IPersistentDataRepository>();
        var persistentData = persistentDataRepository.GetData();

        if (persistentData.NextScheduleParseDateTime[typeof(T).Name] > DateTimeOffset.Now)
        {
            return;
        }

        _logger.LogInformation("Beginning daily parsing of the schedule at {Time}", DateTime.Now);

        var (modifiedEntries, lessons) = await _scheduleReader.ReadSchedule(_cancellationTokenSource.Token);

        foreach (var modifiedEntry in modifiedEntries)
        {
            _repository.RemoveByKey(modifiedEntry.GetKey());
        }
        _repository.AddRange(lessons);
        _modifiedRepository.PushModifiedEntries(modifiedEntries);

        await _modifiedRepository.SaveChangesAsync(_cancellationTokenSource.Token);
        await _repository.SaveChangesAsync(_cancellationTokenSource.Token);

        _logger.LogInformation("Finished parsing schedule at {Time}", DateTime.Now);

        persistentData.NextScheduleParseDateTime[typeof(T).Name] = DateTimeOffset.Now.AddHours(24);
        persistentDataRepository.SetData(persistentData);
        await persistentDataRepository.SaveChangesAsync(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource.Dispose();
        await _timer.DisposeAsync();
    }
}