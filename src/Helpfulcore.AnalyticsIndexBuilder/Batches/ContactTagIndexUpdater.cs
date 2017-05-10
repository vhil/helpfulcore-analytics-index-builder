namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Analytics.Model.Entities;
    using Sitecore.ContentSearch.Analytics.Models;

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

        protected override IEnumerable<Tuple<string, ITag, Guid>> LoadSourceEntries(IEnumerable<IContact> sources)
        {
            return sources.Where(contact => contact?.Tags != null).SelectMany(contact => contact.Tags.Entries.Keys
                .Where(tagKey => contact.Tags.Entries[tagKey] != null)
                .Select(tagKey => new Tuple<string, ITag, Guid>(
                    tagKey,
                    contact.Tags.Entries[tagKey],
                    contact.Id.Guid)));
        }

        protected override ContactTagIndexable ConstructIndexable(Tuple<string, ITag, Guid> source)
        {
            return new ContactTagIndexable(source.Item1, source.Item2, source.Item3);
        }
    }
}