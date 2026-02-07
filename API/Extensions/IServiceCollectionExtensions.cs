using DataAccess.Repositories.Interfaces;

namespace API.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddRepository<TInterface, TImplementation, TItem>(this IServiceCollection services)
        where TInterface : class, IRepository<TItem>
        where TImplementation : class, TInterface
    {
        return services
            .AddScoped<IRepository<TItem>, TImplementation>()
            .AddScoped<TInterface, TImplementation>();
    }
}