namespace JobSymphony
{
    public record Job : BaseJob
    {
        public Action? Action { get; private set; }

        public Func<Task>? Task { get; private set; }

        public Job(Action action) => Action = action;

        public Job(Func<Task> task) => Task = task;

        public override async Task Run(CancellationToken cancellationToken = default)
        {
            if (Action is not null)
            {
                await System.Threading.Tasks.Task.Run(Action.Invoke, cancellationToken);
            }

            else
            {
                await Task.Invoke().WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
