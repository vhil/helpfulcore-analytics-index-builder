namespace Helpfulcore.AnalyticsIndexBuilder.Updaters
{
    using System.Collections.Generic;
    using ContentSearch;
    using Logging;
    using Sitecore.Analytics.Aggregation.Data.Model;
    using Sitecore.ContentSearch.Analytics.Models;

    public class VisitIndexableUpdater : BatchedIndexableUpdater<IVisitAggregationContext, IVisitAggregationContext, VisitIndexable>
    {
        public VisitIndexableUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int batchSize, 
                int concurrentThreads) 
            : base("type:visit", analyticsSearchService, logger, batchSize, concurrentThreads)
        {
        }

        protected override VisitIndexable ConstructIndexable(IVisitAggregationContext source)
        {
            return new VisitIndexable(source);
        }

        protected override IEnumerable<IVisitAggregationContext> LoadSourceEntries(IEnumerable<IVisitAggregationContext> sources)
        {
            return sources;
        }
    }
}