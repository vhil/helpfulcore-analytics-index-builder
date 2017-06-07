namespace Helpfulcore.AnalyticsIndexBuilder.Updaters
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    
    using Sitecore.Analytics.Model;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.Analytics.Aggregation.Data.Model;

    using ContentSearch;
    using Logging;

    public class VisitPageEventIndexableUpdater : BatchedIndexableUpdater<Tuple<PageEventData, string, Guid, Guid>, IVisitAggregationContext, VisitPageEventIndexable>
    {
        public VisitPageEventIndexableUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int indexSubmitBatchSize, 
                int concurrentThreads) 
            : base("type:visitPageEvent", analyticsSearchService, logger, indexSubmitBatchSize, concurrentThreads)
        {
        }

        protected override VisitPageEventIndexable ConstructIndexable(Tuple<PageEventData, string, Guid, Guid> source)
        {
            return new VisitPageEventIndexable(source.Item1, source.Item2, source.Item3, source.Item4);
        }

        protected override IEnumerable<Tuple<PageEventData, string, Guid, Guid>> LoadSourceEntries(IEnumerable<IVisitAggregationContext> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            return sources.Select(x => x.Visit).Where(visit => visit?.Pages != null)
                .SelectMany(visit => visit.Pages.Where(page => page?.PageEvents != null && !string.IsNullOrEmpty(page.Url?.ToString())).SelectMany(
                    page => page.PageEvents.Where(@event => @event != null).Select(@event => new Tuple<PageEventData, string, Guid, Guid>(
                        @event,
                        page.Url.ToString(), 
                        visit.InteractionId, 
                        visit.ContactId))));
        }
    }
}
