using Core.Queues;
using Core.Scheduling;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Models
{
    /// <summary>
    /// Represents a queue of Jobs that inherit <see cref="IJob"/>. 
    /// </summary>
    public interface IJobQueue
    {
        #region Public Methods 

        /// <summary>
        /// Adds a job <typeparamref name="T"/> to the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A <see cref="ValueTask"/> that returns a <see cref="Guid"/> that represents a job id when completed.</returns>
        public ValueTask<Guid> Add<T>() where T : IJob;

        /// <summary>
        /// Adds a job <typeparamref name="T"/> to the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">Represents parameters that are defined in <typeparamref name="T"/> constructor that is implementing <see cref="IJob"/>. The parameters should be in order as they are defined in the constructor, and any parameters that are resolved by dependency injection (i.e. <see cref="ILogger"/>) should not be included in the parameters</param>
        /// <returns>A <see cref="ValueTask"/> that returns a <see cref="Guid"/> that represents a job id when completed.</returns>
        public ValueTask<Guid> Add<T>(params object[] parameters) where T : IJob;

        /// <summary>
        /// Adds a job <typeparamref name="T"/> with the specified <see cref="JobConfiguration"/> to the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration">The specified job configuration</param>
        /// <param name="parameters">Represents parameters that are defined in <typeparamref name="T"/> constructor that is implementing <see cref="IJob"/>. The parameters should be in order as they are defined in the constructor, and any parameters that are resolved by dependency injection (i.e. <see cref="ILogger"/>) should not be included in the parameters</param>
        /// <returns></returns>
        public ValueTask<Guid> Add<T>(JobConfiguration configuration, params object[] parameters) where T : IJob;

        /// <summary>
        /// Removes a job from the queue. If the job is running, it will be attempted to be cancelled and then removed.
        /// </summary>
        /// <param name="jobId">The job id to remove.</param>
        /// <returns>A <see cref="ValueTask"/> that is completed when the job is removed.</returns>
        public ValueTask Remove(Guid jobId);

        /// <summary>
        /// Gets a job from the queue. 
        /// </summary>
        /// <param name="jobId">The job id to that is used to search for the job</param>
        /// <returns>A <see cref="ValueTask"/> that is completed and returns <see cref="Job"/> if found or <see cref="null"/> if not found.</returns>
        public ValueTask<Job?> Get(Guid jobId);

        #endregion

        #region Internal Methods 

        /// <summary>
        /// Adds a job that is in a <see cref="JobStatus.Pending"/> state that is added to the <see cref="BaseJobQueue._runningJobs"/> concurrent dictionary. 
        /// </summary>
        /// <param name="jobId">The job id to be added to the running jobs queue</param>
        /// <returns>A <see cref="bool"/> that represents if the job was successfully added.</returns>
        internal ValueTask<bool> AddToRunningJobs(Guid jobId);

        /// <summary>
        /// Removes a job that is in a <see cref="JobStatus.Completed"/>, <see cref="JobStatus.Errored"/>, or <see cref="JobStatus.Cancelled"/> state from the <see cref="BaseJobQueue._runningJobs"/> concurrent dictionary.
        /// </summary>
        /// <param name="jobId">The job id to remove.</param>
        /// <returns>A <see cref="bool"/> that represents if the job was successfully removed.</returns>
        internal bool RemoveFromRunningJobs(Guid jobId);

        internal ValueTask<Job?> GetNextJobToEnqueue();

        internal ValueTask<bool> EnqueueJob(Guid jobId);

        internal ValueTask<Job?> DequeueJob();

        internal ValueTask<ReadOnlyCollection<Job>> GetJobsByStatus(JobStatus status, int take = -1);

        internal IAsyncEnumerable<Job> GetJobsAsync(Func<Job, bool> predicate, CancellationToken cancellationToken = default);

        internal ConcurrentDictionary<Guid, JobStatus> GetRunningJobs();

        internal ValueTask ReAddRecurringJob(Guid jobId, Guid recurringId);

        #endregion

    }
}
