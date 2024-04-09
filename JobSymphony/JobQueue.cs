using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Linq;
using JobSymphony.Interfaces;
using System;
using System.Collections.Immutable;

namespace JobSymphony
{
    public sealed class JobQueue
    {
        private readonly List<BaseJob> _queuedJobs = [];
        private readonly ReaderWriterLockSlim _queuedJobsReaderWriterLock = new();
        private readonly BlockingCollection<BaseJob> _pendingJobs = [];
        private readonly ConcurrentDictionary<Guid, BaseJob> _runningJobs = new(capacity: 501, concurrencyLevel: -1);
        public readonly SemaphoreSlim _concurrencySemaphore = new(10, 10);
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<JobQueue> _logger;

        public JobQueue(IServiceScopeFactory serviceScopeFactory, ILogger<JobQueue> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public BaseJob? Get(Guid jobId)
        {
            _queuedJobsReaderWriterLock.EnterReadLock();
            try
            {
                BaseJob? queuedJob = _queuedJobs.SingleOrDefault(job => job.Id == jobId, null);
                BaseJob? pendingJob = _pendingJobs.SingleOrDefault(job => job.Id == jobId, null);
                _runningJobs.TryGetValue(jobId, out BaseJob runningJob);

                if (queuedJob is not null)
                {
                    return queuedJob;
                }
                else if (pendingJob is not null)
                {
                    return pendingJob;
                }
                else if (runningJob is not null)
                {
                    return runningJob;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                _queuedJobsReaderWriterLock?.ExitReadLock();
            }
        }

        public void Remove(Guid jobId)
        {
            _queuedJobsReaderWriterLock.EnterWriteLock();
            try
            {
                BaseJob? queuedJob = _queuedJobs.SingleOrDefault(job => job.Id == jobId, null);

                if (queuedJob is not null)
                {
                    _queuedJobs.Remove(queuedJob);
                    return;
                }
            }
            finally
            {
                _queuedJobsReaderWriterLock.ExitWriteLock();
            }
        }

        #region Queued Jobs

        /// <summary>
        /// Add a job to the queue.
        /// </summary>
        /// <param name="action">The payload the job will run</param>
        /// <returns>A <see cref="Job"/></returns>
        /// <remarks>The <see cref="Job"/> returned will be in a <see cref="JobStatus.Queued"/> state until the job scheduler processes it.</remarks>
        public Job Add(Action action)
        {
            _queuedJobsReaderWriterLock.EnterWriteLock();
            try
            {
                Job job = new(action);
                _queuedJobs.Add(job);
                return job;
            }
            finally
            {
                _queuedJobsReaderWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Add a job to the queue.
        /// </summary>
        /// <param name="action">The payload the job will run</param>
        /// <returns>A <see cref="Job"/></returns>
        /// <remarks>The <see cref="Job"/> returned will be in a <see cref="JobStatus.Queued"/> state until the job scheduler processes it.</remarks>
        public Job Add(Func<Task> task)
        {
            _queuedJobsReaderWriterLock.EnterWriteLock();
            try
            {
                Job job = new(task);
                _queuedJobs.Add(job);
                return job;
            }
            finally
            {
                _queuedJobsReaderWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds a job <typeparamref name="T"/> that implements <see cref="BaseJob"/> to the queue.
        /// </summary>
        /// <returns>A job of type <typeparamref name="T"/></returns>
        /// <remarks>The <see cref="Job"/> returned will be in a <see cref="JobStatus.Queued"/> state until the job scheduler processes it.</remarks>
        public T Add<T>() where T : BaseJob
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
            {
                _queuedJobsReaderWriterLock.EnterWriteLock();
                try
                {
                    _logger.LogInformation("Adding Job {t}", Type.GetType(typeof(T).ToString()).FullName);
                    T job = (T)scope.ServiceProvider.GetRequiredService(Type.GetType(typeof(T).ToString()));
                    _queuedJobs.Add(job);
                    return job;
                }
                finally
                {
                    _queuedJobsReaderWriterLock.ExitWriteLock();
                }
            }
        }

        public ReadOnlyCollection<BaseJob> GetQueuedJobs()
        {
            _queuedJobsReaderWriterLock.EnterReadLock();

            try
            {
                return new List<BaseJob>(_queuedJobs).AsReadOnly();
            }
            finally
            {
                _queuedJobsReaderWriterLock.ExitReadLock();

            }
        }

        #endregion

        #region Pending Jobs

        public async IAsyncEnumerable<BaseJob> ReadAllPendingJobsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            //while (cancellationToken.IsCancellationRequested is not true)
            //{
            //    if (_pendingJobs.TryTake(out BaseJob job) is true)
            //    {
            //        yield return job;
            //    }
            //    else
            //    {
            //        await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            //    }
            //}

            foreach (BaseJob job in _pendingJobs.GetConsumingEnumerable(cancellationToken))
            {
                yield return job;
            };




            _logger.LogCritical("No longer reading any jobs");
        }

        public void EnqueueJob(BaseJob job)
        {
            _queuedJobs.Remove(job);
            _ = _pendingJobs.TryAdd(job);
        }

        #endregion

        #region Running Jobs

        public async Task<bool> AddToRunningJobsList(BaseJob job, CancellationToken cancellationToken = default)
        {
            await _concurrencySemaphore.WaitAsync(cancellationToken);
            return _runningJobs.TryAdd(job.Id, job);
        }

        public void RemoveJobFromRunningJobsList(Guid jobId)
        {
            _runningJobs.Remove(jobId, out _);
            _concurrencySemaphore.Release();
        }

        public int GetRunningJobsCount() => _runningJobs.Count();

        public IEnumerator<KeyValuePair<Guid, BaseJob>> GetRunningJobsEnumerator() => _runningJobs.GetEnumerator();

        #endregion
    }
}
