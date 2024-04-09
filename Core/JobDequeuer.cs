using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;

namespace Core
{
    internal class JobDequeuer(IJobQueue jobQueue, ILogger<JobDequeuer> logger) : BackgroundService
    {
        private readonly IJobQueue _jobQueue = jobQueue;
        private readonly ILogger<JobDequeuer> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Running job dequeuer");

            while (stoppingToken.IsCancellationRequested is not true)
            {
                Job? job = await _jobQueue.GetNextJobToEnqueue();

                if (job is not null)
                {
                    _logger.LogDebug("Next job to pend {iid} - {jobId} - {payload}", job.InsertionId, job.Id, job.Payload);

                    await _jobQueue.EnqueueJob(job.Id);
                }

                await Task.Delay(1000000, stoppingToken);
            }
        }
    }
}
