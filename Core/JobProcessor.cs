using Core.Scheduling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using System.Collections.Concurrent;

namespace Core
{
    internal class JobProcessor(ILogger<JobProcessor> logger, IJobQueue jobQueue, IJobScheduler jobScheduler) : BackgroundService
    {
        private readonly ILogger<JobProcessor> _logger = logger;
        private readonly IJobQueue _jobQueue = jobQueue;
        private readonly IJobScheduler _jobScheduler = jobScheduler;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested is not true)
            {
                ConcurrentDictionary<Guid, JobStatus> runningJobs = _jobQueue.GetRunningJobs();

                if (runningJobs.Count is not 0)
                {
                    foreach (KeyValuePair<Guid, JobStatus> job in runningJobs)
                    {
                        switch (job.Value)
                        {
                            case JobStatus.Running:
                                break;
                            case JobStatus.Completed:
                                _logger.LogInformation("Job {id} has successfully completed", job.Key);
                                _jobQueue.RemoveFromRunningJobs(job.Key);
                                await PostProcess(job.Key);
                                break;
                            default:
                                break;
                        }
                    }
                }

                else
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private async ValueTask PostProcess(Guid jobId)
        {
            Job? job = await _jobQueue.Get(jobId);
            if (job is null)
            {
                return;
            }
            else if (job.Recurring is true)
            {
                await _jobScheduler.RescheduleRecurringJob(jobId, job.RecurringId);
            }          
        }
    }
}
