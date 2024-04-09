using Models;
using System.Collections.Concurrent;

namespace Core.Queues
{
    internal class BaseJobQueue(JobQueueConfigurationOptions configurationOptions)
    {
        internal readonly SemaphoreSlim _concurrencySemaphore = new(configurationOptions.Concurrency, configurationOptions.Concurrency);
        internal readonly ConcurrentDictionary<Guid, JobStatus> _runningJobs = new();
    }
}
