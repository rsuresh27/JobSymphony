using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSymphony.ServiceExtensions
{
    public static class ServiceExtensions
    {
        public static IJobSchedulerConfiguration AddJobScheduler(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddSingleton<JobQueue>();
            //serviceCollection.AddSingleton<JobScheduler>();
            //serviceCollection.AddHostedService(provider => provider.GetRequiredService<JobScheduler>());
            //serviceCollection.AddSingleton<JobProcessor>();
            //serviceCollection.AddHostedService(provider => provider.GetRequiredService<JobProcessor>());
            //serviceCollection.AddSingleton<JobRunner>();
            //serviceCollection.AddHostedService(provider => provider.GetRequiredService<JobRunner>());
            serviceCollection.AddSingleton<JobScheduler>();
            serviceCollection.AddHostedService(provider => provider.GetRequiredService<JobScheduler>());

            return new JobSchedulerConfiguration(serviceCollection);
        }
    }
}
