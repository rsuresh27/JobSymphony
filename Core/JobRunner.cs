using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;

namespace Core
{
    internal class JobRunner(IJobQueue jobQueue, ILogger<JobRunner> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        private readonly ILogger<JobRunner> _logger = logger;
        private readonly IJobQueue _jobQueue = jobQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested is not true)
            {
                Job? job = await _jobQueue.DequeueJob();
                if (job is not null)
                {
                    await _jobQueue.AddToRunningJobs(job.Id);

                    await RunJob(job.Id, job.Payload, job.PayloadArgs, stoppingToken);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task RunJob(Guid jobId, string payload, string[]? payloadArgs, CancellationToken stoppingToken)
        {
            try
            {
                using (AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    // if a job has arguments, instantiate the object and pass the args
                    if (payloadArgs is not null)
                    {
                        object[] convertedParamsToCorrectTypes = payloadArgs.Select(ConvertToCorrectDataType).ToArray();

                        IJob job = (IJob)ActivatorUtilities.CreateInstance(scope.ServiceProvider, Type.GetType(payload)!, convertedParamsToCorrectTypes);
                        await job.Run(stoppingToken);
                    }
                    else
                    {
                        var job = scope.ServiceProvider.GetRequiredKeyedService<IJob>(payload);
                        await job.Run();
                    }

                    bool updated = _jobQueue.GetRunningJobs().TryUpdate(jobId, JobStatus.Completed, JobStatus.Running);
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _jobQueue.GetRunningJobs().TryUpdate(jobId, JobStatus.Running, JobStatus.Errored);
            }
        }

        private object ConvertToCorrectDataType(string value)
        {
            if (int.TryParse(value, out int intResult) is true)
            {
                return intResult;
            }

            else if (bool.TryParse(value, out bool boolResult) is true)
            {
                return boolResult;
            }

            else if (double.TryParse(value, out double doulbeResult) is true)
            {
                return doulbeResult;
            }

            else if (float.TryParse(value, out float floatResult) is true)
            {
                return floatResult;
            }

            else if (DateTime.TryParse(value, out DateTime datetimeResult))
            {
                return datetimeResult;
            }

            else
            {
                return value;
            }
        }
    }
}
