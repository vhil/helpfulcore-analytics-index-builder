namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System.Collections.Generic;
    using Sitecore.ContentSearch;

    /// <summary>
    /// Abstraction for analytics search service. Supposed to work with sitecore analytics content search index.
    /// </summary>
    public interface IAnalyticsSearchService : ILoggerChangeable
    {
        /// <summary>
        /// Executes faceted search query to analytics content search index on indexable type.
        /// </summary>
        /// <returns>
        /// Returns aggregated facet search result as <see cref="AnalyticsIndexablesFacetResult"/>
        /// </returns>
        AnalyticsIndexablesFacetResult GetAnalyticsIndexFacets();

        /// <summary>
        /// Builds indexables into content search documents and submits them to the analytics index.
        /// </summary>
        /// <param name="indexables">
        /// IEnumerable of indexables to submit to analytics index.
        /// </param>
        void UpdateIndexables(IEnumerable<AbstractIndexable> indexables);

        /// <summary>
        /// Deletes all indexables from analytics index by specified indexable type.
        /// </summary>
        /// <param name="indexableType">
        /// Indexable type all entries of which are going to be deleted from index. By default next types are available : 'contact', 'contactTag', 'address', 'visit', 'visitPage', 'visitPageEvent'
        /// </param>
        void DeleteIndexablesByType(string indexableType);

        /// <summary>
        /// Resets the sitecore analytics content search index. This action erases all data in the index.
        /// </summary>
        void ResetIndex();
    }
}