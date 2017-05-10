namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System.Collections.Generic;
    using Sitecore.ContentSearch;

    public interface IAnalyticsSearchService : ILoggerChangeable
    {
        AnalyticsEntryFacetResult GetAnalyticsIndexFacets();
        void UpdateIndexables(IEnumerable<AbstractIndexable> contacts);
    }
}