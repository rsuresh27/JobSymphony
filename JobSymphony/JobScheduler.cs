using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSymphony
{
    public class JobScheduler(ILogger<JobScheduler> logger, JobQueue jobQueue) : BackgroundService
    {
        private readonly ILogger<JobScheduler> _logger = logger;
        private readonly JobQueue _jobQueue = jobQueue;

        public Job Add(Func<Task> task) => _jobQueue.Add(task);

        public Job Add(Action action) => _jobQueue.Add(action);

        public T Add<T>() where T : BaseJob => _jobQueue.Add<T>();

        public void Cancel(Guid jobId)
        {
            BaseJob? job = _jobQueue.Get(jobId);
            if (job is not null)
            {
                // if the job is running, let the processor handle cancelling it 
                // if the job is pending, let the scheduler handle it
                if (job.Status is JobStatus.Running || job.Status is JobStatus.Pending)
                {
                    job.Cancel();
                }
                // if a job is in the queued queue that needs to be cancelled, remove it 
                else
                {
                    _logger.LogInformation("Removing...");
                    _jobQueue.Remove(jobId);
                }
                _logger.LogInformation("Job {id} has successfully been cancelled", jobId);
            }
            else
            {
                _logger.LogError("Could not find job {id}", jobId);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested is not true)
            {
                foreach (BaseJob job in _jobQueue.GetQueuedJobs().Where(job => job.ScheduledTime <= DateTime.Now).ToList())
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    // if the job has been requested to be cancelled, remove the job and continue to the next job
                    if (job.Status is JobStatus.Cancelled)
                    {
                        _jobQueue.Remove(job.Id);
                        continue;
                    }
                    _jobQueue.EnqueueJob(job);
                }

                await Task.Delay(100);
            }
        }
    }
}
