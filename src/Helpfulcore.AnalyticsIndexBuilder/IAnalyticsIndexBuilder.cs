namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;
    using Logging;

    public interface IAnalyticsIndexBuilder : ILoggerChangeable
    {
        bool IsBusy { get; }

        void RebuildContactEntriesIndex();
        void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds);

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitEntriesIndex();

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitPageEntriesIndex();

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitPageEventEntriesIndex();

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildAddressEntriesIndex();
    }
}