namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using System.Collections.Generic;

    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Analytics.Models;
    using Logging;

    public interface IAnalyticsSearchService : ILoggerChangeable
    {
        AnalyticsEntryFacetResult GetAnalyticsIndexFacets();
        IEnumerable<IndexedContact> GetIndexedContacts(IEnumerable<Guid> contactIds = null);
        void UpdateIndexables(IEnumerable<AbstractIndexable> contacts);
        void RemoveContactsFromIndex(IEnumerable<AbstractIndexable> contacts);
    }
}