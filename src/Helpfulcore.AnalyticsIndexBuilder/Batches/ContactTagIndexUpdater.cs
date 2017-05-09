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

    public class ContactTagIndexUpdater : BatchedEntryIndexUpdater<Tuple<string, ITag, Guid>, ContactTagIndexable>
    {
        public ContactTagIndexUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int batchSize, 
                int concurrentThreads) 
            : base("type:contactTag", analyticsSearchService, logger, batchSize, concurrentThreads)
        {
        }

        protected override ICollection<Tuple<string, ITag, Guid>> GetAllSourceEntries(ICollection<Guid> contactIds)
        {
            var contacts = contactIds
                .Select(id => DataAdapterManager.Provider.LoadContactReadOnly(new ID(id), this.ContactFactory));

            return contacts.SelectMany(c => c.Tags.Entries.Keys
                .Where(tagKey => c.Tags.Entries[tagKey] != null)
                .Select(tagKey => new Tuple<string, ITag, Guid>(tagKey, c.Tags.Entries[tagKey], c.Id.Guid)))
                .ToList();
        }

        protected override ContactTagIndexable ConstructIndexable(Tuple<string, ITag, Guid> source)
        {
            return new ContactTagIndexable(source.Item1, source.Item2, source.Item3);
        }
    }
}
