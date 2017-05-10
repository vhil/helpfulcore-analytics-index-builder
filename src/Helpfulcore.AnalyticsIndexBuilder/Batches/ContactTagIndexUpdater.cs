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

    public class ContactTagIndexUpdater : BatchedEntryIndexUpdater<Tuple<string, ITag, Guid>, IContact, ContactTagIndexable>
    {
        public ContactTagIndexUpdater(
                IAnalyticsSearchService analyticsSearchService, 
                ILoggingService logger, 
                int batchSize, 
                int concurrentThreads) 
            : base("type:contactTag", analyticsSearchService, logger, batchSize, concurrentThreads)
        {
        }

        public override IEnumerable<Tuple<string, ITag, Guid>> LoadSourceEntries(IEnumerable<IContact> sources)
        {
            return sources.SelectMany(this.LoadSourceEntries);
        }

        public override IEnumerable<Tuple<string, ITag, Guid>> LoadSourceEntries(IContact source)
        {
            return source.Tags.Entries.Keys
                .Where(tagKey => source.Tags.Entries[tagKey] != null)
                .Select(tagKey => new Tuple<string, ITag, Guid>(tagKey, source.Tags.Entries[tagKey], source.Id.Guid));
        }

        public override IEnumerable<Tuple<string, ITag, Guid>> GetAllSourceEntries(IEnumerable<Guid> contactIds)
        {
            return new ConcurrentLazyContactIterator(contactIds
                    .Select(id => DataAdapterManager.Provider.LoadContactReadOnly(new ID(id), this.ContactFactory)))
                .SelectMany(this.LoadSourceEntries);
        }

        protected override ContactTagIndexable ConstructIndexable(Tuple<string, ITag, Guid> source)
        {
            return new ContactTagIndexable(source.Item1, source.Item2, source.Item3);
        }
    }
}