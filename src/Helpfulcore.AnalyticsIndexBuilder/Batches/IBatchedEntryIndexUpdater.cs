using System;

namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System.Collections.Generic;

    public interface IBatchedEntryIndexUpdater : ILoggerChangeable
    {
        string IndexableType { get; }
        void ProcessInBatches(IEnumerable<object> parentObjects);
        long Updated { get; }
        long Failed { get; }
        event Action StatusChanged;
    }
}