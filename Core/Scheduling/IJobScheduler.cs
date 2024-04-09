using Models;

namespace Core.Scheduling
{
    public interface IJobScheduler
    {
        public ValueTask<Guid> Schedule<T>() where T : IJob;

        public ValueTask<Guid> Schedule<T>(params object[] parameters) where T : IJob;

        public ValueTask<Guid> ScheduleWithConfiguration<T>(JobConfiguration configuration, params object[] parameters) where T : IJob;

        internal ValueTask RescheduleRecurringJob(Guid jobId, Guid recurringId);
    }
}
