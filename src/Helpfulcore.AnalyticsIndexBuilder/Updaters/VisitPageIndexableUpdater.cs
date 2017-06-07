namespace Helpfulcore.AnalyticsIndexBuilder.Updaters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using Sitecore.Analytics.Model;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.Analytics.Aggregation.Data.Model;

    using ContentSearch;
    using Logging;

    public class VisitPageIndexableUpdater : BatchedIndexableUpdater<Tuple<PageData, Guid, Guid>, IVisitAggregationContext, VisitPageIndexable>
    {
        public VisitPageIndexableUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int indexSubmitBatchSize, 
                int concurrentThreads) 
            : base("type:visitPage", analyticsSearchService, logger, indexSubmitBatchSize, concurrentThreads)
        {
        }

        protected override VisitPageIndexable ConstructIndexable(Tuple<PageData, Guid, Guid> sourse)
        {
            return new VisitPageIndexable(sourse.Item1, sourse.Item2, sourse.Item3);
        }

        protected override IEnumerable<Tuple<PageData, Guid, Guid>> LoadSourceEntries(IEnumerable<IVisitAggregationContext> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            return sources.Select(x => x.Visit).Where(visit => visit?.Pages != null).SelectMany(visit => visit.Pages.Where(page => page != null).Select(
                p => new Tuple<PageData, Guid, Guid>(p, visit.ContactId, visit.InteractionId)));
        }
    }
}
