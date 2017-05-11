namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;

    public interface IAnalyticsIndexBuilder : ILoggerChangeable
    {
        bool IsBusy { get; }

        void RebuildAllIndexables(bool applyFilters);

        void RebuildContactIndexableTypes(bool applyFilters);
        void RebuildContactIndexableTypes(IEnumerable<Guid> contactIds);

        void RebuildVisitIndexableTypes(bool applyFilters);
        void RebuildVisitIndexableTypes(IEnumerable<Guid> contactIds);

        void RebuildContactIndexables(bool applyFilters);
        void RebuildContactIndexables(IEnumerable<Guid> contactIds);

        void RebuildAddressIndexables(bool applyFilters);
        void RebuildAddressIndexables(IEnumerable<Guid> contactIds);

        void RebuildContactTagIndexables(bool applyFilters);
        void RebuildContactTagIndexables(IEnumerable<Guid> contactIds);

        void RebuildVisitIndexables(bool applyFilters);
        void RebuildVisitIndexables(IEnumerable<Guid> contactIds);

        void RebuildVisitPageIndexables(bool applyFilters);
        void RebuildVisitPageIndexables(IEnumerable<Guid> contactIds);

        void RebuildVisitPageEventIndexables(bool applyFilters);
        void RebuildVisitPageEventIndexables(IEnumerable<Guid> contactIds);
    }
}