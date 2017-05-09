using Helpfulcore.Logging;

namespace Helpfulcore.AnalyticsIndexBuilder
{
    public interface ILoggerChangeable
    {
        void ChangeLogger(ILoggingService logger);
    }
}
