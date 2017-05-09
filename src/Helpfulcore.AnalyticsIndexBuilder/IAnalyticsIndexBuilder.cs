namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;

    public interface IAnalyticsIndexBuilder : ILoggerChangeable
    {
        bool IsBusy { get; }

        void RebuildContactEntriesIndex();
        void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildAddressEntriesIndex();
        void RebuildAddressEntriesIndex(IEnumerable<Guid> contactIds);

        void RebuildContactTagEntriesIndex();
        void RebuildContactTagEntriesIndex(IEnumerable<Guid> contactIds);

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitEntriesIndex();

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitPageEntriesIndex();

        [Obsolete("Not implemented at the moment.", true)]
        void RebuildVisitPageEventEntriesIndex();
    }
}