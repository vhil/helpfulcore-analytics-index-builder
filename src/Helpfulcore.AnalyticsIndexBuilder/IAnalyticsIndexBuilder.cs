namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;

    public interface IAnalyticsIndexBuilder : ILoggerChangeable
    {
        bool IsBusy { get; }
        void RebuildAllEntriesIndexe(bool applyFilters);

        void RebuildContactEntriesIndex(bool applyFilters);
        void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildAddressEntriesIndex(bool applyFilters);
        void RebuildAddressEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildContactTagEntriesIndex(bool applyFilters);
        void RebuildContactTagEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildVisitEntriesIndex(bool applyFilters);
        void RebuildVisitEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildVisitPageEntriesIndex(bool applyFilters);
        void RebuildVisitPageEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildVisitPageEventEntriesIndex(bool applyFilters);
        void RebuildVisitPageEventEntriesIndex(IEnumerable<Guid> contactIds);
    }
}