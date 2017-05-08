namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using System.Collections.Generic;
    using Sitecore.ContentSearch.Analytics.Models;
    using Logging;

    public interface IAnalyticsSearchService : ILoggerChangeable
    {
        AnalyticsEntryFacetResult GetAnalyticsIndexFacets();
        IEnumerable<IndexedContact> GetIndexedContacts(IEnumerable<Guid> contactIds = null);
        void UpdateContactsInIndex(IEnumerable<ContactIndexable> contacts);
        void RemoveContactsFromIndex(IEnumerable<IndexedContact> contacts);
    }
}