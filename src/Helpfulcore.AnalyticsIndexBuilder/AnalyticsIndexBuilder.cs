namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Sitecore.Analytics.Data.DataAccess;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Configuration;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.Data;

    using ContactSelection;
    using ContentSearch;
    using Logging;

    public class AnalyticsIndexBuilder : IAnalyticsIndexBuilder
    {
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly IContactsSelectorProvider ContactSelector;
        protected readonly ILoggingService Logger;
        protected readonly int BatchSize;

        public AnalyticsIndexBuilder(
                IAnalyticsSearchService analyticsSearchService,
                IContactsSelectorProvider contactSelector,
                ILoggingService logger,
                string batchSize = "500") 
            : this(analyticsSearchService, contactSelector, logger, int.Parse(batchSize))
        {
        }

        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService, 
            IContactsSelectorProvider contactSelector, 
            ILoggingService logger, 
            int batchSize = 500)
        {
            this.AnalyticsSearchService = analyticsSearchService;
            this.ContactSelector = contactSelector;
            this.Logger = logger;
            this.BatchSize = batchSize;
        }

        public virtual bool IsBusy { get; protected set; }

        public virtual void RebuildContactEntriesIndex()
        {
            this.SafeExecution("rebuilding type:contact entries index", () =>
            {
                var contactIds = this.ContactSelector.GetContactIdsToReindex();
                this.UpdateContactsIndex(contactIds.Distinct());
            });
        }

        public virtual void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as Guid[] ?? contactIds.ToArray();

            this.SafeExecution($"rebuilding type:contact entries index for {ids.Length} contacts", () =>
            {
                this.UpdateContactsIndex(ids.Distinct());
            });
        }

        public virtual void RebuildVisitEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:visit entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        public virtual void RebuildVisitPageEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:visitPage entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        public virtual void RebuildVisitPageEventEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:visitPageEvent entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        public virtual void RebuildAddressEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:address entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        protected virtual void UpdateContactsIndex(IEnumerable<Guid> contactIds)
        {
            var contacts = contactIds as Guid[] ?? contactIds.ToArray();

            var dbContacts = new List<IContact>();

            var factory = Factory.CreateObject("model/entities/contact/factory", true) as IContactFactory;

            long count = 0;

            this.Logger.Info($"Updating contact indexables progress: {count} of {contacts.Length} (0.00%)", this);

            foreach (var contactId in contacts.Distinct())
            {
                dbContacts.Add(DataAdapterManager.Provider.LoadContactReadOnly(new ID(contactId), factory));
                count++;

                if (count % this.BatchSize == 0 || count == contacts.Length)
                {
                    var indexables = this.LoadContactFields(dbContacts);

                    var indexedContacts = this.AnalyticsSearchService.GetIndexedContacts(indexables.Keys);
                    this.AnalyticsSearchService.RemoveContactsFromIndex(indexedContacts);
                    this.AnalyticsSearchService.UpdateContactsInIndex(indexables.Values);

                    var percentage = 100 * count / (decimal)contacts.Length;
                    this.Logger.Info($"Updating contact indexables progress: {count} of {contacts.Length} ({percentage:#0.00}%).", this);

                    dbContacts.Clear();
                }
            }
        }

        protected virtual IDictionary<Guid, ContactIndexable> LoadContactFields(IEnumerable<IContact> dbContacts)
        {
            // loading contact indexables could be a heavy operation 
            // so executing it in multiple threads for performance

            var indexables = new ConcurrentDictionary<Guid, ContactIndexable>();

            Parallel.ForEach(dbContacts, contact =>
            {
                if (!indexables.ContainsKey(contact.Id.Guid))
                {
                    // this will execute "contactindexable.loadfields" pipeline to load field values;
                    // creating ContactIndexable without visit context will take previously stored visit data from mongo.
                    var indexable = new ContactIndexable(contact);

                    indexables.TryAdd(contact.Id.Guid, indexable);
                }
            });

            return indexables;
        }

        protected virtual void SafeExecution(string actionDescription, Action action)
        {
            if (this.IsBusy)
            {
                this.Logger.Warn($"Unable to execute {actionDescription}. AnalyticsIndexBuilder is busy at the moment with another operation.", this);
                return;
            }

            this.Logger.Info($"Start {actionDescription}. Batch size is set to {this.BatchSize}...", this);

            try
            {
                this.IsBusy = true;
                action.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error while {actionDescription}. {ex.Message}", this, ex);
            }
            finally
            {
                this.IsBusy = false;
                this.Logger.Info($"DONE {actionDescription}.", this);
            }
        }
    }
}
