namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ContactSelection;
    using ContentSearch;
    using Batches;
    using Logging;

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
            IContactsSelectorProvider contactSelector,
            ILoggingService logger,
            int batchSize = 500,
            int concurrentThreads = 4)
        {
            if (analyticsSearchService == null) throw new ArgumentNullException(nameof(analyticsSearchService));
            if (contactSelector == null) throw new ArgumentNullException(nameof(contactSelector));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this.AnalyticsSearchService = analyticsSearchService;
            this.ContactSelector = contactSelector;
            this.Logger = logger;
            this.BatchSize = batchSize;
            this.ConcurrentThreads = concurrentThreads;
        }

        public virtual bool IsBusy { get; protected set; }

        public virtual void RebuildContactEntriesIndex()
        {
            var indexUpdater = new ContactIndexUpdater(
                this.AnalyticsSearchService, 
                this.Logger, 
                this.BatchSize, 
                this.ConcurrentThreads);

            this.SafeExecution($"rebuilding {indexUpdater.IndexableType} entries index", () =>
            {
                var contactIds = this.ContactSelector.GetContactIdsToReindex();
                indexUpdater.ProcessInBatches(contactIds);
            });
        }

        public virtual void RebuildContactEntriesIndex(IEnumerable<Guid> contactIds)
        {
            var indexUpdater = new ContactIndexUpdater(
                this.AnalyticsSearchService, 
                this.Logger, 
                this.BatchSize, 
                this.ConcurrentThreads);

            this.SafeExecution($"rebuilding {indexUpdater.IndexableType}  entries index for {contactIds.Count()} contacts", () =>
            {
                indexUpdater.ProcessInBatches(contactIds);
            });
        }

        public virtual void RebuildAddressEntriesIndex()
        {
            var indexUpdater = new AddressIndexUpdater(
                this.AnalyticsSearchService,
                this.Logger,
                this.BatchSize,
                this.ConcurrentThreads);

            this.SafeExecution($"rebuilding {indexUpdater.IndexableType} entries index", () =>
            {
                var contactIds = this.ContactSelector.GetContactIdsToReindex();
                indexUpdater.ProcessInBatches(contactIds);
            });
        }

        public virtual void RebuildAddressEntriesIndex(IEnumerable<Guid> contactIds)
        {
            var indexUpdater = new AddressIndexUpdater(
                this.AnalyticsSearchService,
                this.Logger,
                this.BatchSize,
                this.ConcurrentThreads);

            this.SafeExecution($"rebuilding {indexUpdater.IndexableType} entries index", () =>
            {
                indexUpdater.ProcessInBatches(contactIds);
            });
        }

        public virtual void RebuildContactTagEntriesIndex()
        {
            var indexUpdater = new ContactTagIndexUpdater(
                this.AnalyticsSearchService,
                this.Logger,
                this.BatchSize,
                this.ConcurrentThreads);

            this.SafeExecution($"rebuilding {indexUpdater.IndexableType} entries index", () =>
            {
                var contactIds = this.ContactSelector.GetContactIdsToReindex();
                indexUpdater.ProcessInBatches(contactIds);
            });
        }

        public virtual void RebuildContactTagEntriesIndex(IEnumerable<Guid> contactIds)
        {
            var indexUpdater = new ContactTagIndexUpdater(
                this.AnalyticsSearchService,
                this.Logger,
                this.BatchSize,
                this.ConcurrentThreads);

            this.SafeExecution($"rebuilding {indexUpdater.IndexableType} entries index", () =>
            {
                indexUpdater.ProcessInBatches(contactIds);
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

        #endregion

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
