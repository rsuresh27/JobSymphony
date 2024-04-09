namespace Models
{
    /// <summary>
    /// The base interface for a job. All jobs that are defined must inherit and implement this interface.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// The logic of the job. This will be run whenever the job is executed. 
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the execution of the function, thus cancelling the job.</param>
        /// <returns>A task that completes when the method completes.</returns>
        public abstract ValueTask Run(CancellationToken cancellationToken = default);
    }
}
