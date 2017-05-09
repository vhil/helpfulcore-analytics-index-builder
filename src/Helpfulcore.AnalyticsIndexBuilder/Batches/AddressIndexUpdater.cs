namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Analytics.Data.DataAccess;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Analytics.Model.Framework;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.Data;

    using Logging;
    using ContentSearch;

    public class AddressIndexUpdater : BatchedEntryIndexUpdater<Tuple<string, Guid, IAddress>, AddressIndexable>
    {
        public AddressIndexUpdater(
            IAnalyticsSearchService analyticsSearchService, 
            ILoggingService logger, 
            int batchSize, 
            int concurrentThreads) : base("type:address", analyticsSearchService, logger, batchSize, concurrentThreads)
        {
        }

        protected override ICollection<Tuple<string, Guid, IAddress>> GetAllSourceEntries(ICollection<Guid> contactIds)
        {
            var sourceEntries = new List<Tuple<string, Guid, IAddress>>();

            this.Logger.Info($"Loading contact addresses for {contactIds.Count} contacts...", this);

            foreach (var contactId in contactIds)
            {
                var contact = DataAdapterManager.Provider.LoadContactReadOnly(new ID(contactId), this.ContactFactory);
                var addresses = this.GetContactAddresses(contact);
                sourceEntries.AddRange(addresses.Select(address => new Tuple<string, Guid, IAddress>(address.Key, contactId, address.Value)));
            }

            return sourceEntries;
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
    }
}
