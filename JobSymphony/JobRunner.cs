using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace JobSymphony
{
    public sealed class JobRunner(ILogger<JobRunner> logger, JobQueue jobQueue) : BackgroundService
    {
        private readonly ILogger<JobRunner> _logger = logger;
        private readonly JobQueue _jobQueue = jobQueue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (BaseJob job in _jobQueue.ReadAllPendingJobsAsync(stoppingToken))
                {
                    try
                    {
                        if (job.Status is JobStatus.Cancelled)
                        {
                            _logger.LogInformation("skip cancelled job");
                            continue;
                        }

                        bool added = await _jobQueue.AddToRunningJobsList(job, stoppingToken);
                        if (added is true)
                        {
                            _logger.LogInformation("Running job {id}", job.Id);
                            _ = RunJob(job, stoppingToken, job.GetJobCancellationToken());
                        }
                        else
                        {
                            _logger.LogError("A job with id {id} already exists and is currently running", job.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        // this will catch the exception thrown from AddToRunningJobsList
                        if (ex is OperationCanceledException)
                        {

                        }
                        else
                        {
                            _logger.LogError(ex, "An error occurred trying to run job {id}: {ex}", job.Id, ex.Message);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Waiting for any running jobs to complete...");
            await base.StopAsync(cancellationToken);
        }

        private async Task RunJob(BaseJob job, CancellationToken cancellationToken, CancellationToken individualJobCancelltationToken)
        {
            try
            {
                job.UpdateJobStatus(JobStatus.Running);
                CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, individualJobCancelltationToken);
                await job.Run(cancellationTokenSource.Token).ConfigureAwait(false);
                job.UpdateJobStatus(JobStatus.Completed);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    job.UpdateJobStatus(JobStatus.Cancelled);
                }
                else
                {
                    _logger.LogError(ex, "An error has ocurred while running job {id}", job.Id);
                    job.UpdateJobStatus(JobStatus.Errored);
                }
            }
        }
    }
}
