using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSymphony
{
    public class JobProcessor(ILogger<JobProcessor> logger, JobQueue jobQueue) : BackgroundService
    {
        private readonly ILogger<JobProcessor> _logger = logger;
        private readonly JobQueue _jobQueue = jobQueue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested is not true)
            {
                await ProcessRunningJobs(1000);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            while (_jobQueue.GetRunningJobsCount() is not 0)
            {
                await ProcessRunningJobs(100);
            }
            await base.StopAsync(cancellationToken);
        }

        private async Task ProcessRunningJobs(int processingTimeout)
        {
            //_logger.LogInformation(_jobQueue.GetRunningJobsCount().ToString());
            IEnumerator<KeyValuePair<Guid, BaseJob>> runningJobs = _jobQueue.GetRunningJobsEnumerator();

            while (runningJobs.MoveNext())
            {
                KeyValuePair<Guid, BaseJob> job = runningJobs.Current;
                switch (job.Value.Status)
                {
                    case JobStatus.Completed:
                        _logger.LogInformation("Successfully completed job {id}", job.Key);
                        _jobQueue.RemoveJobFromRunningJobsList(job.Key);
                        break;
                    case JobStatus.Cancelled:
                        _logger.LogInformation("Job {id} has been cancelled", job.Key);
                        _jobQueue.RemoveJobFromRunningJobsList(job.Key);
                        break;
                    case JobStatus.Errored:
                        _jobQueue.RemoveJobFromRunningJobsList(job.Key);
                        break;
                    default:
                        break;
                }
            }

            await Task.Delay(processingTimeout);
        }
    }
}

