using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniOps.Nucleus.Redis.Config;
using MiniOps.Nucleus.Redis.Stores;
using Nucleus.Core.Config;
using Nucleus.Core.Services;
using Nucleus.Core.Stores;

namespace MiniOps.Nucleus.Redis.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddNucleusRedis(this IServiceCollection services, Action<NucleusRedisOptions> configure)
    {
        var redisOptions = new NucleusRedisOptions();
        configure(redisOptions);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRequestStore));
        if (descriptor != null)
            services.Remove(descriptor);

        // Add Redis store instead
        services.AddSingleton<IRequestStore>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RedisRequestStore>>();
            return new RedisRequestStore(redisOptions.ConnectionString, logger);
        });

        return services;
    }
}