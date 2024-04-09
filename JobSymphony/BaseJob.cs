using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobSymphony
{
    public abstract record BaseJob
    {
        [Required]
        [Key]
        public Guid Id { get; } = Guid.NewGuid();

        [Required]
        public DateTime CreatedTime { get; } = DateTime.Now;

        [Required]
        public DateTime ScheduledTime { get; set; } = DateTime.Now;

        [Required]
        public JobStatus Status { get; private set; } = JobStatus.Pending;

        public void UpdateJobStatus(JobStatus jobStatus) => Status = jobStatus;

        [Required]
        private CancellationTokenSource _cancellationTokenSource = new();

        public CancellationToken GetJobCancellationToken() => _cancellationTokenSource.Token;

        public virtual void Cancel()
        {
            _cancellationTokenSource.Cancel();
            Status = JobStatus.Cancelled;
        }

        public abstract Task Run(CancellationToken cancellationToken = default);

    }
}