namespace Helpfulcore.AnalyticsIndexBuilder.Logging
{
    using Helpfulcore.Logging;

    public interface ILoggerChangeable
    {
        void ChangeLogger(ILoggingService logger);
    }
}
