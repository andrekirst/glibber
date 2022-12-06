using Microsoft.Extensions.DependencyInjection;

namespace Library.Redis;

public static class RegisterCacheClient
{
    public static IServiceCollection AddCacheClient(this IServiceCollection services)
    {
        services.AddSingleton<ICacheClient, CacheClient>();
        return services;
    }
}