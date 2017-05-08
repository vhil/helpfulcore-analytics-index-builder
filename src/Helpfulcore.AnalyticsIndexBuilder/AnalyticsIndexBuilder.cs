namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    using Sitecore.Analytics.Data.DataAccess;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Configuration;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.Data;

    using ContactSelection;
    using ContentSearch;
    using Helpfulcore.Logging;

    public class AnalyticsIndexBuilder : IAnalyticsIndexBuilder
    {
        protected readonly int BatchSize;
        protected readonly int ConcurrentThreads;
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly IContactsSelectorProvider ContactSelector;
        protected ILoggingService Logger;

        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService, 
            IContactsSelectorProvider contactSelector, 
            ILoggingService logger,
            string batchSize,
            string concurrentThreads): this(analyticsSearchService, contactSelector, logger, int.Parse(batchSize), int.Parse(concurrentThreads))
        {
        }

        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService,
            IContactsSelectorProvider contactSelector,
            ILoggingService logger,
            int batchSize,
            int concurrentThreads)
        {
            this.AnalyticsSearchService = analyticsSearchService;
            this.ContactSelector = contactSelector;
            this.Logger = logger;
            this.BatchSize = batchSize;
            this.ConcurrentThreads = concurrentThreads;
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

        #region to implement

        [Obsolete("Not implemented at the moment.", true)]
        public virtual void RebuildVisitEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:visit entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        [Obsolete("Not implemented at the moment.", true)]
        public virtual void RebuildVisitPageEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:visitPage entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        [Obsolete("Not implemented at the moment.", true)]
        public virtual void RebuildVisitPageEventEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:visitPageEvent entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        [Obsolete("Not implemented at the moment.", true)]
        public virtual void RebuildAddressEntriesIndex()
        {
            this.SafeExecution($"rebuilding type:address entries index", () =>
            {
                throw new NotImplementedException();
            });
        }

        #endregion

        protected virtual void UpdateContactsIndex(IEnumerable<Guid> contactIds)
        {
            var contacts = contactIds as Guid[] ?? contactIds.ToArray();

            var dbContacts = new List<IContact>();

            var factory = Factory.CreateObject("model/entities/contact/factory", true) as IContactFactory;

            long count = 0;
            long updated = 0;
            long failed = 0;
            this.Logger.Info($"Updating contact indexables progress: {count} of {contacts.Length} (0.00%). Updated: {updated}, Failed: {failed}", this);

            foreach (var contactId in contacts.Distinct())
            {
                dbContacts.Add(DataAdapterManager.Provider.LoadContactReadOnly(new ID(contactId), factory));
                count++;

                if (count % this.BatchSize == 0 || count == contacts.Length)
                {
                    try
                    { 
                        var indexables = this.LoadContactFields(dbContacts);
                        var indexedContacts = this.AnalyticsSearchService.GetIndexedContacts(dbContacts.Select(c => c.Id.Guid));
                        this.AnalyticsSearchService.RemoveContactsFromIndex(indexedContacts);
                        this.AnalyticsSearchService.UpdateContactsInIndex(indexables);

                        updated += dbContacts.Count;
                    }
                    catch(Exception ex)
                    {
                        failed += dbContacts.Count;
                        this.Logger.Error($"Error while updating batch of {dbContacts.Count} contact indexables. {ex.Message}", this);
                    }
                    finally
                    {
                        var percentage = 100 * count / (decimal)contacts.Length;
                        this.Logger.Info($"Updating contact indexables progress: {count} of {contacts.Length} ({percentage:#0.00}%). Updated: {updated}, Failed: {failed}", this);

                        dbContacts.Clear();
                    }
                }
            }
        }

        protected virtual IEnumerable<ContactIndexable> LoadContactFields(IEnumerable<IContact> dbContacts)
        {
            // loading contact indexables could be a heavy operation 
            // so executing it in multiple threads for performance

            var indexables = new ConcurrentBag<ContactIndexable>();

            var options = new ParallelOptions {MaxDegreeOfParallelism = this.ConcurrentThreads};
            Parallel.ForEach(dbContacts.Distinct(), options, contact =>
            {
                // this will execute "contactindexable.loadfields" pipeline to load field values;
                // creating ContactIndexable without visit context will take previously stored visit data from mongo.
                var indexable = new ContactIndexable(contact);

                indexables.Add(indexable);
            });

            return indexables.ToArray();
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

        public void ChangeLogger(ILoggingService logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
                this.AnalyticsSearchService.ChangeLogger(logger);
                this.ContactSelector.ChangeLogger(logger);
            }
        }
    }
}
