namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System.Collections.Generic;

    using Sitecore.Analytics.Model.Entities;
    using Sitecore.ContentSearch.Analytics.Models;

    using ContentSearch;
    using Logging;

    public class ContactIndexUpdater : BatchedEntryIndexUpdater<IContact, IContact, ContactIndexable>
    {
        public ContactIndexUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int batchSize, 
                int concurrentThreads) 
            : base("type:contact", analyticsSearchService, logger, batchSize, concurrentThreads)
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
