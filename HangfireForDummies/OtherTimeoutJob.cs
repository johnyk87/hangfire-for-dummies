namespace HangfireForDummies
{
    public class OtherTimeoutJob : TimeoutJob
    {
        public OtherTimeoutJob(ILogger<OtherTimeoutJob> logger)
            : base(logger)
        {
        }
    }
}