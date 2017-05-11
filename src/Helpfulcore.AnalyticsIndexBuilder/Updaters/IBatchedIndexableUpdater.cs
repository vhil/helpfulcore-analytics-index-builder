namespace Helpfulcore.AnalyticsIndexBuilder.Updaters
{
    using System;
    using System.Collections.Generic;

    public interface IBatchedIndexableUpdater : ILoggerChangeable
    {
        string IndexableType { get; }
        void ProcessInBatches(IEnumerable<object> parentObjects);
        long Updated { get; }
        long Failed { get; }
        event Action StatusChanged;
    }
}