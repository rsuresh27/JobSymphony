using Microsoft.Extensions.Logging;
using Models;

namespace Core.Scheduling
{
    internal class JobScheduler(IJobQueue jobQueue, ILogger<JobScheduler> logger) : IJobScheduler
    {
        private readonly ILogger<JobScheduler> _logger = logger;
        internal readonly IJobQueue _jobQueue = jobQueue;

        /// <inheritdoc cref="IJobQueue.Add{T}()"/>
        public async ValueTask<Guid> Schedule<T>() where T : IJob
        {
            return await _jobQueue.Add<T>();
        }

        /// <inheritdoc cref="IJobQueue.Add{T}(object[])"/>
        public async ValueTask<Guid> Schedule<T>(params object[] parameters) where T : IJob
        {
            return await _jobQueue.Add<T>(parameters);
        }

        public async ValueTask<Guid> ScheduleWithConfiguration<T>(JobConfiguration configuration, params object[] parameters) where T : IJob
        {
            return await _jobQueue.Add<T>(configuration, parameters);
        }

        public async ValueTask RescheduleRecurringJob(Guid jobId, Guid recurringId)
        {
            _logger.LogInformation("Re-adding job {id} to the queue since it is recurring", jobId);
            await _jobQueue.ReAddRecurringJob(jobId, recurringId);
        }
    }
}

