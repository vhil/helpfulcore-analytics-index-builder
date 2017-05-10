namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System.Collections.Generic;

    public interface IBatchedEntryIndexUpdater : ILoggerChangeable
    {
        string IndexableType { get; }
        void ProcessInBatches(IEnumerable<object> parentObjects);
    }
}
