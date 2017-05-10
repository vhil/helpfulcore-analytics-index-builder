namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;

    public interface IAnalyticsIndexBuilder : ILoggerChangeable
    {
        bool IsBusy { get; }

        void RebuildContactEntriesIndex(bool applyFilters);
        void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildAddressEntriesIndex(bool applyFilters);
        void RebuildAddressEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildContactTagEntriesIndex(bool applyFilters);
        void RebuildContactTagEntriesIndex(IEnumerable<Guid> contactIds);
        void RebuildAllEntriesIndexes(bool applyFilters);

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitEntriesIndex();

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitPageEntriesIndex();

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitPageEventEntriesIndex();

    }
}