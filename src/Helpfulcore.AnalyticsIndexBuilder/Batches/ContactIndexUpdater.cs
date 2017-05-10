namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Analytics.Data.DataAccess;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.Data;
    using Collections;

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

        public override IEnumerable<IContact> LoadSourceEntries(IEnumerable<IContact> sources)
        {
            return sources;
        }

        public override IEnumerable<IContact> LoadSourceEntries(IContact source)
        {
            yield return source;
        }

        public override IEnumerable<IContact> GetAllSourceEntries(IEnumerable<Guid> contactIds)
        {
            return new ConcurrentLazyContactIterator(contactIds
                .Select(id => DataAdapterManager.Provider.LoadContactReadOnly(new ID(id), this.ContactFactory)));
        }

        protected override ContactIndexable ConstructIndexable(IContact source)
        {
            return new ContactIndexable(source);
        }
    }
}
