using Core.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Models
{
    [PrimaryKey("InsertionId")]

    public class Job
    {
        public int InsertionId { get; private set; }

        public Guid Id { get; internal set; }

        public DateTime CreatedTime { get; init; }

        public DateTime ScheduledTime { get; internal set; }

        public JobStatus Status { get; internal set; }

        public required string Payload { get; set; }

        public string[]? PayloadArgs { get; internal set; }

        public bool Recurring { get; internal set; }

        public Guid RecurringId { get; internal set; }

        public double Interval { get; internal set; }

        public void Cancel() => Status = JobStatus.Cancelled;

        public void UpdateJobStatus(JobStatus jobStatus) => Status = jobStatus;

        #region Constructors

        [SetsRequiredMembers]
        public Job(string payload)
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTime.Now;
            ScheduledTime = DateTime.Now;
            Status = JobStatus.Pending;
            Payload = payload;
            PayloadArgs = null;
            Recurring = false;
            RecurringId = Guid.Empty;
            Interval = 0;
        }

        [SetsRequiredMembers]
        public Job(string payload, JobConfiguration configuration)
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTime.Now;
            ScheduledTime = configuration.ScheduledTime ?? DateTime.Now;
            Status = JobStatus.Pending;
            Payload = payload;
            Recurring = configuration.Recurring ?? false;
            RecurringId = Recurring is true ? Guid.NewGuid() : Guid.Empty;
            Interval = configuration.Interval!.Value.TotalSeconds;
        }

        [SetsRequiredMembers]
        public Job(string payload, JobConfiguration configuration, Guid recurringId)
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTime.Now;
            ScheduledTime = DateTime.Now.AddSeconds(configuration.Interval!.Value.TotalSeconds);
            Status = JobStatus.Pending;
            Payload = payload;
            Recurring = configuration.Recurring ?? false;
            RecurringId = recurringId;
            Interval = configuration.Interval!.Value.TotalSeconds;
        }

        [SetsRequiredMembers]
        public Job(string payload, string[] payloadArgs)
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTime.Now;
            ScheduledTime = DateTime.Now;
            Status = JobStatus.Pending;
            Payload = payload;
            PayloadArgs = payloadArgs;
            Recurring = false;
            RecurringId = Guid.Empty;
            Interval = 0;
        }

        [SetsRequiredMembers]
        public Job(string payload, string[] payloadArgs, JobConfiguration configuration)
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTime.Now;
            ScheduledTime = configuration.ScheduledTime ?? DateTime.Now;
            Status = JobStatus.Pending;
            Payload = payload;
            PayloadArgs = payloadArgs;
            Recurring = configuration.Recurring ?? false;
            RecurringId = Recurring is true ? Guid.NewGuid() : Guid.Empty;
            Interval = configuration.Interval!.Value.TotalSeconds;
        }

        [SetsRequiredMembers]
        public Job(string payload, string[] payloadArgs, JobConfiguration configuration, Guid recurringId)
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTime.Now;
            ScheduledTime = DateTime.Now.AddSeconds(configuration.Interval!.Value.TotalSeconds);
            Status = JobStatus.Pending;
            Payload = payload;
            PayloadArgs = payloadArgs;
            Recurring = configuration.Recurring ?? true;
            RecurringId = recurringId;
            Interval = configuration.Interval!.Value.TotalSeconds;
        }

        #endregion
    }
}
