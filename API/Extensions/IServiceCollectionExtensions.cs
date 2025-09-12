using BusinessLogic.Jobs;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models.Interface;
using DataAccess.Repositories.Interfaces;

namespace API.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddScheduleSource<TLesson, TLessonModified>(this IServiceCollection services)
        where TLesson: class
        where TLessonModified: class, IModifiedEntry
    {
        return services
            .AddScoped<IScheduleParser<TLesson>>()
            .AddScoped<IScheduleReader<TLesson, TLessonModified>>()
            .AddScoped<ILessonUpdaterService<TLesson, TLessonModified>>()
            .AddScoped<ILessonUpdaterService<TLesson, TLessonModified>>()
            .AddHostedService<ScheduleParserJob<TLesson,TLessonModified>>()
            .AddHostedService<LessonUpdaterJob<TLesson,TLessonModified>>();
    }

    public static IServiceCollection AddRepository<TInterface, TImplementation, TItem>(this IServiceCollection services)
        where TInterface : class, IRepository<TItem>
        where TImplementation : class, TInterface
    {
        return services
            .AddScoped<IRepository<TItem>, TImplementation>()
            .AddScoped<TInterface, TImplementation>();
    }
}