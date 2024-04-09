using Core.Queues.InMemory;
using Core.Queues.SQLite;
using Core.Queues.SQLite.Contexts;
using Core.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using System.Reflection;

namespace Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJobScheduler(this IServiceCollection services)
        {
            services.AddSingleton<IJobScheduler, JobScheduler>();
            services.AddSingleton<JobDequeuer>();
            services.AddHostedService(provider => provider.GetRequiredService<JobDequeuer>());
            services.AddSingleton<JobRunner>();
            services.AddHostedService(provider => provider.GetRequiredService<JobRunner>());
            services.AddSingleton<JobProcessor>();
            services.AddHostedService(provider => provider.GetRequiredService<JobProcessor>());

            return services;
        }

        public static IServiceCollection UseSQLiteDb(this IServiceCollection services, JobQueueConfigurationOptions? jobQueueConfigurationOptions = default)
        {
            services.AddDbContext<JobDbContext>();
            services.AddSingleton<IJobQueue>(provider => new JobQueueSQLiteDb(provider.GetRequiredService<IServiceScopeFactory>(), jobQueueConfigurationOptions!, provider.GetRequiredService<ILogger<JobQueueSQLiteDb>>()));
            return services;
        }

        public static IServiceCollection UseInMemory(this IServiceCollection services, JobQueueConfigurationOptions? jobQueueConfigurationOptions = default)
        {
            services.AddSingleton<IJobQueue>(provider => new JobQueueInMemory(jobQueueConfigurationOptions!));
            return services;
        }

        public static IServiceCollection ConfigureJobs(this IServiceCollection services)
        {
            // get all the classes that implement the ijob interface and register them for dependency injection
            Type[] types = Assembly.GetEntryAssembly()!.GetTypes().Where(type => typeof(IJob).IsAssignableFrom(type) && type.IsInterface is not true).ToArray();

            foreach (Type type in types)
            {
                // use assembly qualified name since jobs are defined in different projects and DI can't resolve dependencies in different assemblies without assembly name
                services.AddKeyedTransient(typeof(IJob), type.AssemblyQualifiedName, type);
            }

            return services;
        }
    }
}
