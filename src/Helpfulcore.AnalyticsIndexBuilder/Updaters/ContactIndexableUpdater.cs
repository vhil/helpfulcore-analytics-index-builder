namespace Helpfulcore.AnalyticsIndexBuilder.Updaters
{
    using System.Collections.Generic;

    using Sitecore.Analytics.Model.Entities;
    using Sitecore.ContentSearch.Analytics.Models;

    using ContentSearch;
    using Logging;

    public class ContactIndexableUpdater : BatchedIndexableUpdater<IContact, IContact, ContactIndexable>
    {
        public ContactIndexableUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int indexSubmitBatchSize, 
                int concurrentThreads) 
            : base("type:contact", analyticsSearchService, logger, indexSubmitBatchSize, concurrentThreads)
        {
        }

        protected override IEnumerable<IContact> LoadSourceEntries(IEnumerable<IContact> sources)
        {
            return sources;
        }

        protected override ContactIndexable ConstructIndexable(IContact source)
        {
            return new ContactIndexable(source);
        }
    }
}
