namespace Helpfulcore.AnalyticsIndexBuilder.Updaters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Analytics.Model.Framework;
    using Sitecore.ContentSearch.Analytics.Models;

    using Logging;
    using ContentSearch;

    public class AddressIndexableUpdater : BatchedIndexableUpdater<Tuple<string, Guid, IAddress>, IContact, AddressIndexable>
    {
        public AddressIndexableUpdater(
            IAnalyticsSearchService analyticsSearchService, 
            ILoggingService logger, 
            int batchSize, 
            int concurrentThreads) : base("type:address", analyticsSearchService, logger, batchSize, concurrentThreads)
        {
        }

        protected override AddressIndexable ConstructIndexable(Tuple<string, Guid, IAddress> source)
        {
            return new AddressIndexable(source.Item1, source.Item2, source.Item3);
        }

        protected virtual Dictionary<string, IAddress> GetContactAddresses(IFaceted contact)
        {
            var dictionary = new Dictionary<string, IAddress>();
            var facet = contact.Facets.FirstOrDefault(kvp => kvp.Value is IContactAddresses).Value;

            if (facet != null)
            {
                var contactAddresses = (IContactAddresses)facet;
                foreach (var key in contactAddresses.Entries.Keys)
                {
                    dictionary.Add(key, contactAddresses.Entries[key]);
                }
            }

            return dictionary;
        }

        protected override IEnumerable<Tuple<string, Guid, IAddress>> LoadSourceEntries(IEnumerable<IContact> sourses)
        {
            return sourses.SelectMany(this.LoadSourceEntries);
        }

        protected virtual IEnumerable<Tuple<string, Guid, IAddress>> LoadSourceEntries(IContact sourse)
        {
            return this.GetContactAddresses(sourse).Select(address => 
                new Tuple<string, Guid, IAddress>(address.Key, sourse.Id.Guid, address.Value));
        }
    }
}
