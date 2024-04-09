namespace Core.Scheduling
{
    public class JobConfiguration
    {
        /// <summary>
        /// Set the time for the job to run at.
        /// </summary>
        public DateTime? ScheduledTime { get; init; } = DateTime.Now;

        /// <summary>
        /// Set the job to run repeatedly forever.
        /// </summary>
        public bool? Recurring { get; init; } = false;

        /// <summary>
        /// Set the job to run at a set interval.
        /// </summary>
        public TimeSpan? Interval { get; init; } = TimeSpan.Zero;

    }
}
