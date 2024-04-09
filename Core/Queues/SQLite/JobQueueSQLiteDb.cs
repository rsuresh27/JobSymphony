using Core.Queues.SQLite.Contexts;
using Core.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Core.Queues.SQLite
{
    internal class JobQueueSQLiteDb(IServiceScopeFactory serviceScopeFactory, JobQueueConfigurationOptions configurationOptions, ILogger<JobQueueSQLiteDb> logger) : BaseJobQueue(configurationOptions), IJobQueue
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<JobQueueSQLiteDb> _logger = logger;

        public async ValueTask<Guid> Add<T>() where T : IJob
        {
            using (JobDbContext jobContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                //Job job = new() { Id = Guid.NewGuid(), Payload = typeof(T).AssemblyQualifiedName!, ScheduledTime = DateTime.Now, CreatedTime = DateTime.Now, Status = JobStatus.Queued };
                Job job = new(typeof(T).AssemblyQualifiedName!);
                _logger.LogDebug("Adding job with id {id}", job.Id);
                await jobContext.Jobs.AddAsync(job);
                await jobContext.SaveChangesAsync();
                return job.Id;
            }
        }

        public async ValueTask<Guid> Add<T>(params object[] parameters) where T : IJob
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                Job job = new(new(typeof(T).AssemblyQualifiedName), parameters.Select(parameter => parameter.ToString()).ToArray()!);
                await jobDbContext.Jobs.AddAsync(job);
                await jobDbContext.SaveChangesAsync();
                return job.Id;
            }
        }

        public async ValueTask<Guid> Add<T>(JobConfiguration configuration, params object[] parameters) where T : IJob
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                //Job job = new() { Id = Guid.NewGuid(), Payload = typeof(T).AssemblyQualifiedName!, ScheduledTime = DateTime.Now, CreatedTime = DateTime.Now, Status = JobStatus.Queued, PayloadArgs = parameters.Select(parameter => parameter.ToString()).ToArray()!, Recurring = configuration.Recurring ?? false };
                Job job = new(typeof(T).AssemblyQualifiedName!, parameters.Select(parameter => parameter.ToString()).ToArray()!, configuration);
                await jobDbContext.Jobs.AddAsync(job);
                await jobDbContext.SaveChangesAsync();
                return job.Id;
            }
        }

        public async ValueTask<bool> AddToRunningJobs(Guid jobId)
        {
            _concurrencySemaphore.Wait();

            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                Job job = await jobDbContext.Jobs.AsQueryable().SingleAsync(job => job.Id == jobId);
                _logger.LogInformation("Running job {id}", job.Id);
                job.Status = JobStatus.Running;
                await jobDbContext.SaveChangesAsync();
                return _runningJobs.TryAdd(jobId, JobStatus.Running);
            }
        }

        public async ValueTask<Job?> DequeueJob()
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                try
                {
                    Job job = await jobDbContext.Jobs.AsQueryable().Where(job => job.ScheduledTime <= DateTime.Now && job.Status == JobStatus.Pending).OrderBy(job => job.InsertionId).FirstAsync();
                    return job;
                }
                catch (InvalidOperationException)
                {
                    _logger.LogInformation("No jobs are queued to run right now");
                    return null;
                }
            }
        }

        public async ValueTask<bool> EnqueueJob(Guid jobId)
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                Job job = await jobDbContext.Jobs.AsQueryable().SingleAsync(job => job.Id == jobId);
                if (job is not null)
                {
                    job.UpdateJobStatus(JobStatus.Pending);
                    await jobDbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async ValueTask<Job?> Get(Guid jobId)
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                return await jobDbContext.Jobs.AsQueryable().SingleOrDefaultAsync(job => job.Id == jobId);
            }
        }

        public async ValueTask<Job?> GetNextJobToEnqueue()
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                try
                {
                    Job job = await jobDbContext.Jobs.AsQueryable().Where(job => job.ScheduledTime <= DateTime.Now && job.Status == JobStatus.Queued).OrderBy(job => job.InsertionId).FirstAsync();
                    return job;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public async IAsyncEnumerable<Job> GetJobsAsync(Func<Job, bool>? predicate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                await foreach (Job job in jobDbContext.Jobs.AsAsyncEnumerable())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (predicate is not null)
                    {
                        if (predicate(job) is true)
                        {
                            yield return job;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        yield return job;
                    }
                }
            }
        }

        public async ValueTask<ReadOnlyCollection<Job>> GetJobsByStatus(JobStatus status, int take = -1)
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                return take == -1 ?
              (await jobDbContext.Jobs.AsQueryable().Where(job => job.Status == status).OrderBy(job => job.InsertionId).ToListAsync()).AsReadOnly() :
              (await jobDbContext.Jobs.AsQueryable().Where(job => job.Status == status).OrderBy(job => job.InsertionId).Take(take).ToListAsync()).AsReadOnly();
            }
        }

        public ConcurrentDictionary<Guid, JobStatus> GetRunningJobs() => _runningJobs;

        public async ValueTask Remove(Guid jobId)
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                Job? job = await jobDbContext.Jobs.AsQueryable().SingleOrDefaultAsync(job => job.Id == jobId);
                if (job is not null)
                {
                    jobDbContext.Jobs.Remove(job);
                    await jobDbContext.SaveChangesAsync();
                }
            }
        }

        public bool RemoveFromRunningJobs(Guid jobId)
        {
            return _runningJobs.TryRemove(jobId, out _);
        }

        public async ValueTask ReAddRecurringJob(Guid jobId, Guid recurringId)
        {
            using (JobDbContext jobDbContext = _serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<JobDbContext>())
            {
                Job pastJob = await jobDbContext.Jobs.AsQueryable().SingleAsync(job => job.Id == jobId);
                if (pastJob.PayloadArgs is not null)
                {
                    JobConfiguration configuration = new() { Interval = TimeSpan.FromSeconds(pastJob.Interval), Recurring = pastJob.Recurring };
                    Job job = new(pastJob.Payload, pastJob.PayloadArgs, configuration, pastJob.RecurringId);
                    await jobDbContext.Jobs.AddAsync(job);
                    await jobDbContext.SaveChangesAsync();
                }
                else
                {
                    Job job = new(pastJob.Payload, new() { Recurring = true }, pastJob.RecurringId);
                }
            }
        }
    }
}
