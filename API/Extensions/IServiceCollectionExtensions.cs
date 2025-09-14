using BusinessLogic.Jobs;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models.Interface;
using DataAccess.Repositories.Interfaces;

namespace API.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddScheduleSource<TLesson, TLessonModified, TParser, TReader, TUpdater, TUserUpdater>(this IServiceCollection services)
        where TLesson: class
        where TLessonModified: class, IModifiedEntry
        where TParser: class, IScheduleParser<TLesson>
        where TReader: class, IScheduleReader<TLesson, TLessonModified>
        where TUpdater: class, ILessonUpdaterService<TLesson, TLessonModified>
        where TUserUpdater: class, IUserLessonUpdaterService<TLesson>
    {
        return services
            .AddScoped<IScheduleParser<TLesson>, TParser>()
            .AddScoped<IScheduleReader<TLesson, TLessonModified>, TReader>()
            .AddScoped<ILessonUpdaterService<TLesson, TLessonModified>, TUpdater>()
            .AddScoped<IUserLessonUpdaterService<TLesson>, TUserUpdater>()
            .AddHostedService<ScheduleParserJob<TLesson,TLessonModified>>()
            .AddHostedService<LessonUpdaterJob<TLesson,TLessonModified>>()
            .AddHostedService<UserLessonUpdaterJob<TLesson>>();
    }

    public static IServiceCollection AddRepository<TInterface, TImplementation, TItem>(this IServiceCollection services)
        where TInterface : class, IRepository<TItem>
        where TImplementation : class, TInterface
    {
        return services
            .AddScoped<IRepository<TItem>, TImplementation>()
            .AddScoped<TInterface, TImplementation>();
    }

    public static IServiceCollection AddKeyBasedRepository<TInterface, TImplementation, TItem>(this IServiceCollection services)
        where TInterface : class, IRepository<TItem>
        where TImplementation : class, TInterface, IKeyBasedRepository<TItem>
    {
        return services
            .AddScoped<IRepository<TItem>, TImplementation>()
            .AddScoped<IKeyBasedRepository<TItem>, TImplementation>()
            .AddScoped<TInterface, TImplementation>();
    }
}