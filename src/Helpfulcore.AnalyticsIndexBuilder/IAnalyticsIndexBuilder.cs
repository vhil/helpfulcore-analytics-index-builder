namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;

    public interface IAnalyticsIndexBuilder
    {
        bool IsBusy { get; }

        void RebuildContactEntriesIndex();
        void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildVisitEntriesIndex();
        void RebuildVisitPageEntriesIndex();
        void RebuildVisitPageEventEntriesIndex();
        void RebuildAddressEntriesIndex();
    }
}