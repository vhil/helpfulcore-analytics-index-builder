namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Analytics.Data.DataAccess;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.Data;

    using ContentSearch;
    using Logging;

    public class ContactIndexUpdater : BatchedEntryIndexUpdater<IContact, ContactIndexable>
    {
        public ContactIndexUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int batchSize, 
                int concurrentThreads) 
            : base("type:contact", analyticsSearchService, logger, batchSize, concurrentThreads)
        {
        }

        protected override ICollection<IContact> GetAllSourceEntries(ICollection<Guid> contactIds)
        {
            return contactIds
                .Select(id => DataAdapterManager.Provider.LoadContactReadOnly(new ID(id), this.ContactFactory))
                .ToList();
        }

        protected override ContactIndexable ConstructIndexable(IContact source)
        {
            return new ContactIndexable(source);
        }
    }
}
