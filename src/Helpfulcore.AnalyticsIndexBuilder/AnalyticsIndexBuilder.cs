namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Sitecore.Analytics.Model.Entities;

    using ContactSelection;
    using ContentSearch;
    using Batches;
    using Logging;

    public class AnalyticsIndexBuilder : IAnalyticsIndexBuilder
    {
        protected readonly int BatchSize;
        protected readonly int ConcurrentThreads;
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly ICollectionDataProvider CollectionDataProvider;
        protected ILoggingService Logger;

        private readonly IBatchedEntryIndexUpdater addressUpdater;
        private readonly IBatchedEntryIndexUpdater contactUpdater;
        private readonly IBatchedEntryIndexUpdater contactTagUpdater;
        private readonly IBatchedEntryIndexUpdater visitUpdater;
        private readonly IBatchedEntryIndexUpdater visitPageUpdater;
        private readonly IBatchedEntryIndexUpdater visitPageEventUpdater;

        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService, 
            ICollectionDataProvider contactSelector, 
            ILoggingService logger,
            string batchSize = "500",
            string concurrentThreads = "4"): this(
                analyticsSearchService, 
                contactSelector, 
                logger, 
                int.Parse(batchSize), 
                int.Parse(concurrentThreads))
        {
        }

        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService,
            ICollectionDataProvider contactSelector,
            ILoggingService logger,
            int batchSize = 500,
            int concurrentThreads = 4)
        {
            if (analyticsSearchService == null) throw new ArgumentNullException(nameof(analyticsSearchService));
            if (contactSelector == null) throw new ArgumentNullException(nameof(contactSelector));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this.AnalyticsSearchService = analyticsSearchService;
            this.CollectionDataProvider = contactSelector;
            this.Logger = logger;
            this.BatchSize = batchSize;
            this.ConcurrentThreads = concurrentThreads;

            this.addressUpdater        =        new AddressIndexUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.contactUpdater        =        new ContactIndexUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.contactTagUpdater     =     new ContactTagIndexUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.visitUpdater          =          new VisitIndexUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.visitPageUpdater      =      new VisitPageIndexUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.visitPageEventUpdater = new VisitPageEventIndexUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
        }

        public virtual bool IsBusy { get; protected set; }

        public void RebuildAllEntriesIndexe(bool applyFilters)
        {
            this.SafeExecution($"rebuilding all {(applyFilters ? "filtered " : "")}entries indexes", () =>
            {
                var contactIds = this.LoadContactIds(applyFilters);

                var contacts = this.CollectionDataProvider.GetContacts(contactIds);
                var visits = applyFilters 
                    ? this.CollectionDataProvider.GetVisits(contactIds)
                    : this.CollectionDataProvider.GetVisits();

                var updateTasks = new Action[]
                {
                    () => { this.addressUpdater.ProcessInBatches(contacts); },
                    () => { this.contactUpdater.ProcessInBatches(contacts); },
                    () => { this.contactTagUpdater.ProcessInBatches(contacts); },
                    () => { this.visitUpdater.ProcessInBatches(visits); },
                    () => { this.visitPageUpdater.ProcessInBatches(visits); },
                    () => { this.visitPageEventUpdater.ProcessInBatches(visits); },
                };

                var options = new ParallelOptions { MaxDegreeOfParallelism = updateTasks.Length };
                Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
            });
        }

        public virtual void RebuildContactEntriesIndex(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.contactUpdater, applyFilters);
        }

        public virtual void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.contactUpdater, contactIds);
        }

        public virtual void RebuildAddressEntriesIndex(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.addressUpdater, applyFilters);
        }

        public virtual void RebuildAddressEntriesIndex(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.addressUpdater, contactIds);
        }

        public virtual void RebuildContactTagEntriesIndex(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.contactTagUpdater, applyFilters);
        }

        public virtual void RebuildContactTagEntriesIndex(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.contactTagUpdater, contactIds);
        }

        public void RebuildVisitEntriesIndex(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitUpdater, applyFilters);
        }

        public void RebuildVisitEntriesIndex(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitUpdater, contactIds);
        }

        public void RebuildVisitPageEntriesIndex(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageUpdater, applyFilters);
        }

        public void RebuildVisitPageEntriesIndex(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageUpdater, contactIds);
        }

        public void RebuildVisitPageEventEntriesIndex(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageEventUpdater, applyFilters);
        }

        public void RebuildVisitPageEventEntriesIndex(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageEventUpdater, contactIds);
        }

        #region infrastructure

        protected virtual void SafeRebuildContactBasedIndex(IBatchedEntryIndexUpdater updater, bool applyFilters)
        {
            this.SafeExecution($"rebuilding {(applyFilters ? "filtered " : "")}{updater.IndexableType} entries index", () =>
            {
                var contactIds = this.LoadContactIds(applyFilters);

                updater.ProcessInBatches(this.CollectionDataProvider.GetContacts(contactIds));
            });
        }

        protected virtual void SafeRebuildContactBasedIndex(IBatchedEntryIndexUpdater updater, IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.ToArray();

            this.SafeExecution($"rebuilding {updater.IndexableType} entries index for {ids.Count} contacts", () =>
            {
                updater.ProcessInBatches(this.CollectionDataProvider.GetContacts(ids));
            });
        }

        protected virtual void SafeRebuildVisitBasedIndex(IBatchedEntryIndexUpdater updater, bool applyFilters)
        {
            this.SafeExecution($"rebuilding {(applyFilters ? "filtered " : "")}{updater.IndexableType} entries index", () =>
            {
                var visits = applyFilters
                    ? this.CollectionDataProvider.GetVisits(this.LoadContactIds(true))
                    : this.CollectionDataProvider.GetVisits();

                updater.ProcessInBatches(visits);
            });
        }

        protected virtual void SafeRebuildVisitBasedIndex(IBatchedEntryIndexUpdater updater, IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.ToArray();

            this.SafeExecution($"rebuilding {updater.IndexableType} entries index for {ids.Count} contacts", () =>
            {
                updater.ProcessInBatches(this.CollectionDataProvider.GetVisits(ids));
            });
        }

        protected virtual ICollection<Guid> LoadContactIds(bool applyFilters)
        {
            var contactIds = applyFilters
                ? this.CollectionDataProvider.GetFilteredContactIdsToReindex()
                : this.CollectionDataProvider.GetAllContactIdsToReindex();

            return contactIds as ICollection<Guid> ?? contactIds.ToArray();
        }

        protected virtual IEnumerable<IContact> LoadContacts(bool applyFilters)
        {
            return this.CollectionDataProvider.GetContacts(this.LoadContactIds(applyFilters));
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
                this.CollectionDataProvider.ChangeLogger(logger);
                this.contactUpdater.ChangeLogger(logger);
                this.contactTagUpdater.ChangeLogger(logger);
                this.addressUpdater.ChangeLogger(logger);
                this.visitUpdater.ChangeLogger(logger);
                this.visitPageUpdater.ChangeLogger(logger);
                this.visitPageEventUpdater.ChangeLogger(logger);
            }
        }

        #endregion
    }
}
