namespace HangfireForDummies
{
    using Hangfire;
    using Hangfire.Server;

    public class TimeoutJob
    {
        // This is the number of retries after the first failure, not the total number of run attempts.
        // If we want to run the job a total of 3 times before failing, we need to set `RetryCount = 2`.
        private const int RetryCount = 2;

        // Manual timeout used inside the job.
        private const int ExecuteTimeoutInSeconds = 1 * 60;

        // Concurrent execution timeout, used to wait for a distributed lock.
        private const int DistributedLockTimeoutInSeconds = ExecuteTimeoutInSeconds - 30;

        private readonly ILogger logger;

        public TimeoutJob(ILogger<TimeoutJob> logger)
        {
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = RetryCount)]
        [DisableConcurrentExecution(DistributedLockTimeoutInSeconds)]
        public async Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken = default)
        {
            this.logger.Log(
                LogLevel.Information,
                $"Starting job {nameof(TimeoutJob)} [{{jobId}}]",
                context.BackgroundJob.Id);

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(ExecuteTimeoutInSeconds));

                await Task.Delay(Timeout.Infinite, linkedCts.Token);
            }
            catch (Exception ex)
            {
                this.logger.Log(
                    LogLevel.Error,
                    ex,
                    $"Error during job {nameof(TimeoutJob)} [{{jobId}}]",
                    context.BackgroundJob.Id);

                throw;
            }
            finally
            {
                this.logger.Log(
                    LogLevel.Information,
                    $"Exiting job {nameof(TimeoutJob)} [{{jobId}}]",
                    context.BackgroundJob.Id);
            }
        }
    }
}