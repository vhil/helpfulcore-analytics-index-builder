namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Sitecore.Analytics.Model.Entities;

    using Data;
    using ContentSearch;
    using Updaters;
    using Logging;

    public class AnalyticsIndexBuilder : IAnalyticsIndexBuilder
    {
        protected readonly int BatchSize;
        protected readonly int ConcurrentThreads;
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly ICollectionDataProvider CollectionDataProvider;
        protected ILoggingService Logger;

        private readonly BatchedIndexableUpdater addressUpdater;
        private readonly BatchedIndexableUpdater contactUpdater;
        private readonly BatchedIndexableUpdater contactTagUpdater;
        private readonly BatchedIndexableUpdater visitUpdater;
        private readonly BatchedIndexableUpdater visitPageUpdater;
        private readonly BatchedIndexableUpdater visitPageEventUpdater;

        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService, 
            ICollectionDataProvider contactSelector, 
            ILoggingService logger,
            string batchSize = "1000",
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
            int batchSize = 1000,
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

            this.addressUpdater        =        new AddressIndexableUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.contactUpdater        =        new ContactIndexableUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.contactTagUpdater     =     new ContactTagIndexableUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.visitUpdater          =          new VisitIndexableUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.visitPageUpdater      =      new VisitPageIndexableUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);
            this.visitPageEventUpdater = new VisitPageEventIndexableUpdater(this.AnalyticsSearchService, this.Logger, this.BatchSize, this.ConcurrentThreads);

            this.addressUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.contactUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.contactTagUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.visitUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.visitPageUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.visitPageEventUpdater.StatusChanged += this.OnUpdatersStatusChanged;
        }

        public virtual bool IsBusy { get; protected set; }

        public void RebuildAllIndexables(bool applyFilters)
        {
            this.SafeExecution($"rebuilding all {(applyFilters ? "filtered " : "")}indexables", () =>
            {
                var contactIds = this.LoadContactIds(applyFilters);

                var contacts = this.CollectionDataProvider.GetContacts(contactIds);
                var visits = applyFilters 
                    ? this.CollectionDataProvider.GetVisits(contactIds)
                    : this.CollectionDataProvider.GetVisits();

                var updateTasks = new Action[]
                {
                    () => { this.contactUpdater.ProcessInBatches(contacts); },
                    () => { this.addressUpdater.ProcessInBatches(contacts); },
                    () => { this.contactTagUpdater.ProcessInBatches(contacts); },
                    () => { this.visitUpdater.ProcessInBatches(visits); },
                    () => { this.visitPageUpdater.ProcessInBatches(visits); },
                    () => { this.visitPageEventUpdater.ProcessInBatches(visits); },
                };

                var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };
                Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
            });
        }

        public void RebuildContactIndexableTypes(bool applyFilters)
        {
            this.RebuildContactIndexableTypes(this.LoadContactIds(applyFilters));
        }

        public void RebuildContactIndexableTypes(IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.Distinct().ToList();

            this.SafeExecution($"rebuilding [{this.contactUpdater.IndexableType}, {this.addressUpdater.IndexableType}, {this.contactTagUpdater.IndexableType}] indexables for {ids.Count} contacts", () =>
            {
                var contacts = this.CollectionDataProvider.GetContacts(ids);

                var updateTasks = new Action[]
                {
                    () => { this.addressUpdater.ProcessInBatches(contacts); },
                    () => { this.contactUpdater.ProcessInBatches(contacts); },
                    () => { this.contactTagUpdater.ProcessInBatches(contacts); },
                };

                var options = new ParallelOptions { MaxDegreeOfParallelism = updateTasks.Length };
                Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
            });
        }

        public void RebuildVisitIndexableTypes(bool applyFilters)
        {
            this.RebuildVisitIndexableTypes(applyFilters ? this.LoadContactIds(true) : null);
        }

        public void RebuildVisitIndexableTypes(IEnumerable<Guid> contactIds)
        {
            var all = contactIds == null;
            var ids = contactIds as ICollection<Guid> ?? contactIds?.Distinct().ToList();

            this.SafeExecution($"rebuilding [{this.visitUpdater.IndexableType}, {this.visitPageUpdater.IndexableType}, {this.visitPageEventUpdater.IndexableType}] indexables {(all ? "for all visits in collection database" : $"for {ids.Count} contacts")}", () =>
            {
                var visits = all
                    ? this.CollectionDataProvider.GetVisits()
                    : this.CollectionDataProvider.GetVisits(ids);

                var updateTasks = new Action[]
                {
                    () => { this.visitUpdater.ProcessInBatches(visits); },
                    () => { this.visitPageUpdater.ProcessInBatches(visits); },
                    () => { this.visitPageEventUpdater.ProcessInBatches(visits); },
                };

                var options = new ParallelOptions { MaxDegreeOfParallelism = updateTasks.Length };
                Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
            });
        }

        public virtual void RebuildContactIndexables(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.contactUpdater, applyFilters);
        }

        public virtual void RebuildContactIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.contactUpdater, contactIds);
        }

        public virtual void RebuildAddressIndexables(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.addressUpdater, applyFilters);
        }

        public virtual void RebuildAddressIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.addressUpdater, contactIds);
        }

        public virtual void RebuildContactTagIndexables(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.contactTagUpdater, applyFilters);
        }

        public virtual void RebuildContactTagIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.contactTagUpdater, contactIds);
        }

        public void RebuildVisitIndexables(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitUpdater, applyFilters);
        }

        public void RebuildVisitIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitUpdater, contactIds);
        }

        public void RebuildVisitPageIndexables(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageUpdater, applyFilters);
        }

        public void RebuildVisitPageIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageUpdater, contactIds);
        }

        public void RebuildVisitPageEventIndexables(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageEventUpdater, applyFilters);
        }

        public void RebuildVisitPageEventIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageEventUpdater, contactIds);
        }

        #region infrastructure

        private void ResetAllStats()
        {
            this.contactUpdater.ResetStats();
            this.addressUpdater.ResetStats();
            this.contactTagUpdater.ResetStats();
            this.visitUpdater.ResetStats();
            this.visitPageUpdater.ResetStats();
            this.visitPageEventUpdater.ResetStats();
        }

        private void OnUpdatersStatusChanged()
        {
            var failed = new StringBuilder();
            var updated = new StringBuilder();

            updated.Append($"{this.contactUpdater.IndexableType}: {this.contactUpdater.Updated}, ");
            updated.Append($"{this.addressUpdater.IndexableType}: {this.addressUpdater.Updated}, ");
            updated.Append($"{this.contactTagUpdater.IndexableType}: {this.contactTagUpdater.Updated}, ");
            updated.Append($"{this.visitUpdater.IndexableType}: {this.visitUpdater.Updated}, ");
            updated.Append($"{this.visitPageUpdater.IndexableType}: {this.visitPageUpdater.Updated}, ");
            updated.Append($"{this.visitPageEventUpdater.IndexableType}: {this.visitPageEventUpdater.Updated}");

            this.Logger.Info($"Updated by indexable type: [ {updated.ToString().Replace("type:", "")} ]", this);

            if (this.contactUpdater.Failed > 0
                || this.addressUpdater.Failed > 0
                || this.contactTagUpdater.Failed > 0
                || this.visitUpdater.Failed > 0
                || this.visitPageUpdater.Failed > 0
                || this.visitPageEventUpdater.Failed > 0)
            {
                failed.Append($"{this.contactUpdater.IndexableType}: {this.contactUpdater.Failed},");
                failed.Append($"{this.addressUpdater.IndexableType}: {this.addressUpdater.Failed},");
                failed.Append($"{this.contactTagUpdater.IndexableType}: {this.contactTagUpdater.Failed},");
                failed.Append($"{this.visitUpdater.IndexableType}: {this.visitUpdater.Failed},");
                failed.Append($"{this.visitPageUpdater.IndexableType}: {this.visitPageUpdater.Failed},");
                failed.Append($"{this.visitPageEventUpdater.IndexableType}: {this.visitPageEventUpdater.Failed}");

                this.Logger.Info($"Failed by indexable type: [ {failed.ToString().Replace("type:", "")} ]", this);
            }
        }

        protected virtual void SafeRebuildContactBasedIndex(IBatchedIndexableUpdater updater, bool applyFilters)
        {
            this.SafeExecution($"rebuilding {(applyFilters ? "filtered " : "")}[{updater.IndexableType}] indexables index", () =>
            {
                var contactIds = this.LoadContactIds(applyFilters);

                updater.ProcessInBatches(this.CollectionDataProvider.GetContacts(contactIds));
            });
        }

        protected virtual void SafeRebuildContactBasedIndex(IBatchedIndexableUpdater updater, IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.ToArray();

            this.SafeExecution($"rebuilding [{updater.IndexableType}] indexables index for {ids.Count} contacts", () =>
            {
                updater.ProcessInBatches(this.CollectionDataProvider.GetContacts(ids));
            });
        }

        protected virtual void SafeRebuildVisitBasedIndex(IBatchedIndexableUpdater updater, bool applyFilters)
        {
            this.SafeExecution($"rebuilding {(applyFilters ? "filtered " : "")}[{updater.IndexableType}] indexables index", () =>
            {
                var visits = applyFilters
                    ? this.CollectionDataProvider.GetVisits(this.LoadContactIds(true))
                    : this.CollectionDataProvider.GetVisits();

                updater.ProcessInBatches(visits);
            });
        }

        protected virtual void SafeRebuildVisitBasedIndex(IBatchedIndexableUpdater updater, IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.ToArray();

            this.SafeExecution($"rebuilding [{updater.IndexableType}] indexables index for {ids.Count} contacts", () =>
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

            this.ResetAllStats();

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
