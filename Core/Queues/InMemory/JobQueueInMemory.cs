using Core.Scheduling;
using Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Queues.InMemory
{
    internal class JobQueueInMemory(JobQueueConfigurationOptions configurationOptions) : BaseJobQueue(configurationOptions), IJobQueue
    {
        ValueTask<Guid> IJobQueue.Add<T>()
        {
            throw new NotImplementedException();
        }

        ValueTask<Guid> IJobQueue.Add<T>(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        ValueTask<Guid> IJobQueue.Add<T>(JobConfiguration configuration, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        ValueTask<bool> IJobQueue.AddToRunningJobs(Guid jobId)
        {
            throw new NotImplementedException();
        }

        ValueTask<Job?> IJobQueue.DequeueJob()
        {
            throw new NotImplementedException();
        }

        ValueTask<bool> IJobQueue.EnqueueJob(Guid jobId)
        {
            throw new NotImplementedException();
        }

        ValueTask<Job?> IJobQueue.Get(Guid jobId)
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<Job> IJobQueue.GetJobsAsync(Func<Job, bool> predicate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ValueTask<ReadOnlyCollection<Job>> IJobQueue.GetJobsByStatus(JobStatus status, int take)
        {
            throw new NotImplementedException();
        }

        ValueTask<Job?> IJobQueue.GetNextJobToEnqueue()
        {
            throw new NotImplementedException();
        }

        ConcurrentDictionary<Guid, JobStatus> IJobQueue.GetRunningJobs()
        {
            throw new NotImplementedException();
        }

        ValueTask IJobQueue.Remove(Guid jobId)
        {
            throw new NotImplementedException();
        }

        bool IJobQueue.RemoveFromRunningJobs(Guid jobId)
        {
            throw new NotImplementedException();
        }

        ValueTask IJobQueue.ReAddRecurringJob(Guid jobId, Guid recurringId)
        {
            throw new NotImplementedException();
        }
    }
}
